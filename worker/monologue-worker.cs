using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Rss;

class Settings {
	public static bool Verbose;

	public static void Log (string fmt, params object [] args)
	{
		if (Verbose)
			Console.Error.WriteLine (fmt, args);
	}
}

class MonologueWorker {
	static string bloggersFile;
	static string outputFile;
	static string rssOutFile;
	static DateTime lastReadOfBloggersFile;
	
	static RssFeed outFeed;
	static BloggerCollection bloggers;
	
	static int Main (string [] args)
	{
		if (args.Length < 3 || args.Length > 8) {
			Console.WriteLine ("monologue-worker.exe BLOGGERS_FILE HTML_OUTPUT RSS_OUTPUT " +
						"[--loop [ms to sleep]] [--cachedir dirname] [--verbose]");
			return 1;
		}
		
		bloggersFile = args[0];
		outputFile = args[1];
		rssOutFile = args[2];

		bool loop = false;
		int msToSleep = 0;
		int aLength = args.Length;

		for (int i = 3; i < aLength; i++) {
			if (args [i] == "--verbose") {
				Settings.Verbose = true;
				Console.WriteLine("Processing using the list of bloggers: '{0}'", bloggersFile);
				continue;
			}

			if (args [i] == "--loop") {
				loop = true;
				msToSleep = 600000;
				try {
					msToSleep = int.Parse (args [++i]);
				} catch {
					i--;
				}
				continue;
			}

			if (args [i] == "--cachedir") {
				if (++i >= aLength) {
					Console.WriteLine ("--cachedir needs an argument");
					return 1;
				}

				FeedCache.CacheDir = Path.Combine (Environment.CurrentDirectory, args [i]);
				continue;
			}
			Console.Error.WriteLine ("Invalid argument: {0}", args [i]);
			return 1;
		}
		
		do {
			Thread.Sleep(msToSleep);
			RunOnce();
		} while (loop);

		return 0;
	}

	static int [] counters = new int [(int) UpdateStatus.MAX];
	static int next;
	static Blogger [] all;
	static int loaded;
	delegate ReadResult ReadDelegate (string name, string url);
	static AsyncCallback feed_done = new AsyncCallback (FeedDone);
	static bool disable_load;
	static ManualResetEvent wait_handle = new ManualResetEvent (false);

	static void FeedDone (IAsyncResult ares)
	{
		AsyncResult a = (AsyncResult) ares;
		ReadResult res = null;
		try {
			res = ((ReadDelegate) a.AsyncDelegate).EndInvoke (ares);
		} catch {
			res = new ReadResult (UpdateStatus.Error);
		}

		if (disable_load)
			return;
		Blogger blogger = (Blogger) ares.AsyncState;
		blogger.Feed = res.Feed;
		blogger.UpdateFeed ();
		if (res.Status == UpdateStatus.CachedButError)
			blogger.Error = true;
		Settings.Log ("DONE {0}", blogger.RssUrl);
		lock (all) {
			loaded++;
			counters [(int) res.Status]++;
			if (loaded >= all.Length)
				wait_handle.Set ();
			if (next >= all.Length)
				return;
			Blogger b = all [next++];
			ReadDelegate d = new ReadDelegate (FeedCache.Read);
			d.BeginInvoke (b.Name, b.RssUrl, feed_done, b);
		}
	}

	static void RunOnce ()
	{
		if (bloggers == null || File.GetLastWriteTime (bloggersFile) > lastReadOfBloggersFile) {
			lastReadOfBloggersFile = File.GetLastWriteTime (bloggersFile);
			bloggers = BloggerCollection.LoadFromFile (bloggersFile);
		}
		
		disable_load = false;
		all = (Blogger []) bloggers.Bloggers.ToArray (typeof (Blogger));
		lock (all) {
			next = 10;
			for (int i = 0; i < 10 && i < all.Length; i++) {
				Blogger b = all [i];
				ReadDelegate d = new ReadDelegate (FeedCache.Read);
				d.BeginInvoke (b.Name, b.RssUrl, feed_done, b);
			}
		}

		wait_handle.WaitOne (300000, false);
		disable_load = true;
		
		for (int i = 0; i < (int) UpdateStatus.MAX; i++) {
			Console.WriteLine ("{0}: {1}", (UpdateStatus) i, counters [i]);
		}

		int error = counters [(int) UpdateStatus.Error];
		int downloaded = counters [(int) UpdateStatus.Downloaded];
		int updated = counters [(int) UpdateStatus.Updated];
		if (error == 0 && downloaded == 0 && updated == 0)
			return;

		outFeed = new RssFeed ();
		RssChannel ch = new RssChannel ();
		ch.Title = "Monologue";
		ch.Generator = "Monologue worker: b-diddy powered";
		ch.Description = "The voices of Mono";
		ch.Link = new Uri ("http://www.go-mono.com/monologue/");
		
		ArrayList stories = new ArrayList ();
		
		DateTime minPubDate = DateTime.Now.AddDays (-14);
		foreach (Blogger b in bloggers.BloggersByUrl) {
			if (b.Channel == null) continue;
			foreach (RssItem i in b.Channel.Items) {
				if (i.PubDate == DateTime.MinValue) {
					b.DateError = true;
				} else if (i.PubDate >= minPubDate) {
					i.Title = b.Name + ": " + i.Title;
					i.PubDate = i.PubDate.ToUniversalTime ();
					stories.Add (i);
				}
			}
		}
		
		stories.Sort (new ReverseRssItemComparer ());
		
		foreach (RssItem itm in stories)
			ch.Items.Add (itm);

		if (ch.Items.Count == 0) {
			Settings.Log ("No feeds to store.");
			return;
		}
		outFeed.Channels.Add (ch);

		if (Settings.Verbose)
			Console.WriteLine("Building aggregated feed at '{0}'", rssOutFile);
		
		outFeed.Write(rssOutFile);
		
		Render ();
	}
	
	public class ReverseRssItemComparer : IComparer {
		int IComparer.Compare( Object x, Object y )
		{
			return DateTime.Compare (((RssItem)y).PubDate, ((RssItem)x).PubDate);
		}
	}
	public class ReverseRssItemColComparer : IComparer {
		int IComparer.Compare( Object x, Object y )
		{
			return DateTime.Compare (((RssItem)((ArrayList)y) [0]).PubDate.Date, ((RssItem)((ArrayList)x) [0]).PubDate.Date);
		}
	}

	static string error_msg = "<div class='ircnick' style='color:red'>Error retrieving/loading feed</div>";
	static string date_error_msg = "<div class='ircnick' style='color:red'>Invalid dates in feed</div>";
	static string error_img = "<img src='images/error.png' alt='Error retrieving/loading feed'>";
	
	static string ProcessHeadEntryAndReturnUrl (string head)
	{
		string localHeadsPath = "images/heads/";
		
		if (String.IsNullOrEmpty (head))
			head = "none.png";
		
		if (!(new Uri (head, UriKind.RelativeOrAbsolute).IsAbsoluteUri))
			head = localHeadsPath + head;
		
		return head;
	}
	
	static void Render ()
	{
		string templatefile = "default.tpl";

		if (Settings.Verbose)
			Console.WriteLine("Starting to render '{0}' using template '{1}'", outputFile, templatefile);

		Template tpl = new Template(templatefile);

		tpl.selectSection ("BLOGGER");
		foreach (Blogger b in bloggers.Bloggers) {
			tpl.setField ("BLOGGER_ERROR_MSG", b.Error ? error_msg : b.DateError ? date_error_msg : "");
			tpl.setField ("BLOGGER_ERROR_IMG", b.Error || b.DateError ? error_img : "");
			tpl.setField ("BLOGGER_URL", b.HtmlUrl.ToString ());
			tpl.setField ("BLOGGER_NAME", b.Name);

			tpl.setField ("BLOGGER_HEAD", ProcessHeadEntryAndReturnUrl (b.Head));

			if (b.IrcNick != null)
				tpl.setField ("BLOGGER_IRCNICK", b.IrcNick);
			else
				tpl.setField ("BLOGGER_IRCNICK", String.Empty);

			tpl.setField ("BLOGGER_RSSURL", b.RssUrl);
			
			tpl.appendSection ();
		}
		tpl.deselectSection ();

		tpl.selectSection ("BLOG_DAY");
		foreach (ArrayList day in GetDaysCollectionList ()) {
			
			tpl.setField ("DAY_DATE", ((RssItem)day [0]).PubDate.Date.ToString ("M"));

			tpl.selectSection ("DAY_ENTRY");
			foreach (RssItem itm in day) {
				tpl.setField ("ENTRY_LINK", itm.Link.ToString ());

				Blogger bl = bloggers [itm.Author];
				if (bl != null) {
					tpl.setField ("ENTRY_PERSON", bl.Name);
					
					if (bl.IrcNick != null)
						tpl.setField ("ENTRY_PERSON_IRCNICK", "(" + bl.IrcNick + ")");
					else
						tpl.setField ("ENTRY_PERSON_IRCNICK", "");
					
					tpl.setField ("ENTRY_PERSON_HEAD", ProcessHeadEntryAndReturnUrl (bl.Head));

					tpl.setField ("ENTRY_PERSON_URL", bl.HtmlUrl.ToString());
				} else {
					throw new Exception ("No blogger for " + itm.Author  + ".");
				}

				itm.Title = itm.Title.Substring (itm.Title.IndexOf (":")+2);
				tpl.setField("ENTRY_TITLE", itm.Title);
				tpl.setField ("ENTRY_DATE", itm.PubDate.ToString ("h:mm tt 'GMT'"));
				tpl.setField("ENTRY_HTML", itm.Content ?? itm.Description);

				tpl.appendSection ();
			}
			
			tpl.deselectSection ();
			tpl.appendSection ();
		}
		tpl.deselectSection ();
		
		
		TextWriter w = new StreamWriter (File.Create (outputFile));
		w.Write (tpl.getContent ());
		w.Flush ();
	}
	
	static ArrayList GetDaysCollectionList ()
	{
		Hashtable ht = new Hashtable ();
		
		foreach (RssItem itm in outFeed.Channels [0].Items) {
			ArrayList ar = ht [itm.PubDate.Date] as ArrayList;
			if (ar != null) {
				ar.Add (itm);
			} else {
				ht [itm.PubDate.Date] = ar = new ArrayList ();
				ar.Add (itm);
			}
		}
		
		ArrayList ret = new ArrayList (ht.Values);
		ret.Sort (new ReverseRssItemColComparer ());
		return ret;
	}
}

public class BloggerCollection {
	static XmlSerializer serializer = new XmlSerializer (typeof (BloggerCollection));
	
	public static BloggerCollection LoadFromFile (string file)
	{
		BloggerCollection coll = (BloggerCollection)serializer.Deserialize (new XmlTextReader (file));
		return coll.PrepareForUse();
	}

	ArrayList bloggers;
	ArrayList bloggersByUrl;
	Hashtable idToBlogger;

	[XmlElement ("Blogger", typeof (Blogger))]
	public ArrayList Bloggers {
		get {
			return bloggers;
		}
		set {
			bloggers = value;
		}
	}

	private BloggerCollection PrepareForUse()
	{
		bloggers.Sort(new BloggerComparer());
		idToBlogger = new Hashtable();
		foreach (Blogger b in bloggers)
			idToBlogger.Add(b.ID, b);
		bloggersByUrl = null;
		return this;
	}

	public ArrayList BloggersByUrl {
		get {
			if (bloggersByUrl == null) {
				bloggersByUrl = new ArrayList (bloggers);
				bloggersByUrl.Sort (new UrlComparer ());
			}
			return bloggersByUrl;
		}
	}

	public Blogger this [string id] {
		get {
			return (Blogger)idToBlogger [id];
		}
	}

	public class BloggerComparer : IComparer {
		int IComparer.Compare (object x, object y)
		{
			return String.Compare (((Blogger)x).Name, ((Blogger)y).Name);
		}
	}

	public class UrlComparer : IComparer {
		int IComparer.Compare (object x, object y)
		{
			return String.Compare (((Blogger)x).RssUrl, ((Blogger)y).RssUrl);
		}
	}
}

public enum UpdateStatus {
	Downloaded,
	Updated,
	Cached,
	CachedButError,
	Error,
	MAX
}

public class Blogger {
	[XmlAttribute] public string Name;
	[XmlAttribute] public string RssUrl;
	[XmlAttribute] public string IrcNick;
	[XmlAttribute] public string Head;
	bool error;
	bool date_error;

	[XmlIgnore]
	public string ID {
		// Must look like an email to make rss happy
		get { 
			return XmlConvert.EncodeLocalName (Name) + "@" + XmlConvert.EncodeLocalName ("monologue.go-mono.com"); 
		}
	}
		
	RssFeed feed;
	[XmlIgnore]
	public RssChannel Channel {
		get {
			if (feed == null)
				return null;
		
			if (feed.Channels.Count == 0)
				return null;
	
			return feed.Channels [0];
		}
	}
	
	[XmlIgnore]
	public Uri HtmlUrl {
		get {
			if (Channel != null)
				return Channel.Link;
			return new Uri ("http://www.go-mono.com/monologue");
		}
	}
	
	public RssFeed Feed {
		set { feed = value; }
	}

	public string Author {
		get {
			return XmlConvert.EncodeLocalName (Name) + "@" + XmlConvert.EncodeLocalName ("monologue.go-mono.com");
		}
	}

	public bool DateError {
		get { return (date_error || feed == null); }
		set { date_error = value; }
	}

	public bool Error {
		get { return (error || feed == null); }
		set { error = value; }
	}

	public void UpdateFeed ()
	{
		if (feed == null)
			return;

		// TODO: Do we still need this?
		if (feed.Channels.Count > 0)
			foreach (RssItem i in feed.Channels [0].Items)
				i.Author = Author;
	}
}

class ReadResult {
	UpdateStatus status;
	RssFeed feed;

	public ReadResult (UpdateStatus status)
	{
		this.status = status;
	}

	public ReadResult (RssFeed feed, UpdateStatus status)
	{
		this.feed = feed;
		this.status = status;
	}

	public RssFeed Feed {
		get { return feed; }
	}

	public UpdateStatus Status {
		get { return status; }
	}

}

class FeedCache {
	static string cacheDir;

	public static string CacheDir {
		get { return cacheDir; }
		set {
			cacheDir = value;
			Directory.CreateDirectory (cacheDir);
		}
	}

	static string GetName (string name)
	{
		int hash = name.GetHashCode ();
		if (hash < 0)
			hash = -hash;

		return Path.Combine (cacheDir, hash.ToString ());
	}

	public static ReadResult Read (string name, string url)
	{
		return Read (name, url, 0);
	}

	static ReadResult Read (string name, string url, int redirects)
	{
		if (redirects > 10) {
			Settings.Log ("Too many redirects.");
			return new ReadResult (UpdateStatus.Error);
		}
			
		if (name == null)
			throw new ArgumentNullException ("name");

		Settings.Log ("Starting {0}", url);
		HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
		req.UserAgent = "Monologue";
		req.Timeout = 30000;
		req.ReadWriteTimeout = 30000;
		req.AutomaticDecompression = DecompressionMethods.GZip;

		string filename = null;
		bool exists = false;
		if (Enabled) {
			filename = GetName (name);
			exists = File.Exists (filename);
			if (exists) {
				req.IfModifiedSince = File.GetLastWriteTime (filename);
				 // This way, a 304 (NotModified) is not an error
				req.AllowAutoRedirect = false;
			}
		}

		HttpWebResponse resp = null;
		try {
			resp = (HttpWebResponse) req.GetResponse ();
		} catch (WebException we) {
			if (we.Response != null)
				we.Response.Close ();

			Settings.Log ("1 {0}: {1}", url, we.Message);
			if (exists) { // ==> Enabled
				Settings.Log ("Using the cached file for {0}", url);
				return new ReadResult (RssFeed.Read (filename), UpdateStatus.CachedButError);
			}

			return new ReadResult (UpdateStatus.Error);
		}

		HttpStatusCode code = resp.StatusCode;
		switch (code) {
		case HttpStatusCode.OK: // 200
			break; // go ahead

		case HttpStatusCode.MovedPermanently: // 301
		case HttpStatusCode.Redirect: // 302
		case HttpStatusCode.SeeOther: //303
		case HttpStatusCode.TemporaryRedirect: // 307
			string location = resp.Headers ["Location"];
			resp.Close ();
			try {
				Uri uri = new Uri (new Uri (url), location);
				location = uri.ToString ();
				Settings.Log ("Redirecting from {0} to {1}", req.Address, location);
			} catch (Exception) {
				Settings.Log ("Error in 'Location' header for {0}.", req.Address);
				return new ReadResult (UpdateStatus.Error);
			}

			return Read (name, location, ++redirects);

		case HttpStatusCode.NotModified: // 304
			resp.Close ();
			Settings.Log ("{0} not modified since {1}.", req.Address, req.Headers ["If-Modified-Since"]);
			return new ReadResult (RssFeed.Read (filename), UpdateStatus.Cached);

		default:
			resp.Close ();
			Settings.Log ("2 {0} getting {1}", code, url);

			if (exists) {
				Settings.Log ("Using the cached file for {0}.", url);
				return new ReadResult (RssFeed.Read (filename), UpdateStatus.CachedButError);
			}
			return new ReadResult (UpdateStatus.Error);
		}

		byte [] buffer = new byte [16384];
		int length = buffer.Length;
		Stream input = null;
		try {
			input = resp.GetResponseStream ();
		} catch (WebException we) {
			Settings.Log ("3 getting response stream {0} for {1}: {2}",
					name, url, we.Message);

			if (exists) {
				Settings.Log ("Using the cached file for {0}.", url);
				return new ReadResult (RssFeed.Read (filename), UpdateStatus.CachedButError);
			}

			return new ReadResult (UpdateStatus.Error);
		}

		if (filename == null)
			filename = Path.GetTempFileName ();
		
		try {
			int nread = 0;
			if ((nread = input.Read (buffer, 0, length)) != 0)
			{
				File.Delete (filename);
				using (FileStream f = new FileStream (filename, FileMode.CreateNew))
				{
					do
					{
						f.Write (buffer, 0, nread);
					} while ((nread = input.Read (buffer, 0, length)) != 0);
				}
			}

			return new ReadResult (RssFeed.Read (filename), UpdateStatus.Downloaded);
		} catch (Exception e) {
			Console.Error.WriteLine ("4 {0} reading {1}", e, url);
			return new ReadResult (UpdateStatus.Error);
		} finally {
			resp.Close ();
			if (!Enabled)
				File.Delete (filename);
		}
	}

	public static bool Enabled {
		get { return CacheDir != null; }
	}
}

