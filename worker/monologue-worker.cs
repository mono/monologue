using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Rss;

class MonologueWorker {
	static string bloggersFile;
	static string outputFile;
	static string rssOutFile;
	static DateTime lastReadOfBloggersFile;
	
	
	static RssFeed outFeed;
	static BloggerCollection bloggers;
	
	static void Main (string [] args)
	{
		if (args.Length < 3 || args.Length > 5)
			throw new Exception ("monologue-worker.exe BLOGGERS_FILE HTML_OUTPUT RSS_OUTPUT [--loop [ms to sleep]]");
		
		bloggersFile = args [0];
		outputFile = args [1];
		rssOutFile = args [2];
		if (args.Length < 4 || args [3] != "--loop") {
			RunOnce ();
		} else {
			int msToSleep = 600000;
			if (args.Length >= 5)
				msToSleep = int.Parse (args [4]);
			do {
				RunOnce ();
				System.Threading.Thread.Sleep (msToSleep);
			} while (true);
		}
	}
	
	static void RunOnce ()
	{
		if (bloggers == null || File.GetLastWriteTime (bloggersFile) > lastReadOfBloggersFile) {
			lastReadOfBloggersFile = File.GetLastWriteTime (bloggersFile);
			bloggers = BloggerCollection.LoadFromFile (bloggersFile);
		}
		
		bool somethingChanged = false;
		
		foreach (Blogger b in bloggers.Bloggers)
			somethingChanged |= b.UpdateFeed ();
		
		if (!somethingChanged)
			return;
		
		outFeed = new RssFeed ();
		RssChannel ch = new RssChannel ();
		ch.Title = "Monologue";
		ch.Generator = "Monologue worker: b-diddy powered";
		ch.Description = "The voices of Mono";
		ch.Link = new Uri ("http://monologue.go-mono.com");
		
		ArrayList stories = new ArrayList ();
		
		DateTime minPubDate = DateTime.Now.AddDays (-14);
		foreach (Blogger b in bloggers.Bloggers) {
			if (b.Channel == null) continue;
			foreach (RssItem i in b.Channel.Items) {
				if (i.PubDate >= minPubDate)
					stories.Add (i);
			}
		}
		
		stories.Sort (new ReverseRssItemComparer ());
		
		foreach (RssItem itm in stories)
			ch.Items.Add (itm);
		
		outFeed.Channels.Add (ch);
		outFeed.Write (rssOutFile);
		
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
	
	static void Render ()
	{
		TextWriter w = new StreamWriter (File.Create (outputFile));
		
		w.WriteLine (@"
			<html><head>
				<title>Monologue::</title>
				<link rel='stylesheet' href='monologue.css' type='text/css'>
				<script src='prettyprint.js'>
			</head><body onload='paintColors();'>
			<h1>Monologue</h1>
			");
			
		
		w.WriteLine (@"<div id='bloggers'><h2>Bloggers</h2><ul>");
		foreach (Blogger b in bloggers.Bloggers) {
			w.WriteLine (@"<li><a href='{0}'>{1}</a> <a href='{2}'>(rss)</a></li>", b.HtmlUrl, b.Name, b.RssUrl);
		}
		w.WriteLine (@"</ul></div>");
		
		w.WriteLine (@"<div id='blogs'>");
		foreach (ArrayList day in GetDaysCollectionList ()) {
			w.WriteLine (@"<h2>{0}</h2>", ((RssItem)day [0]).PubDate.Date.ToString ("M"));
			foreach (RssItem itm in day) {
				w.WriteLine (@"
					<h3><a href='{0}'>{1}: {2}</a></h3>
					<div class='blogentry'>
						{3}
						<p>Posted at {4}</p>
					</div>
				", itm.Link, bloggers [itm.Author].Name, itm.Title, itm.Description, itm.PubDate.ToString ("h:mm tt"));
			}
		}
		w.WriteLine (@"</div>");
		w.WriteLine (@"</body></html>");
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
		return (BloggerCollection)serializer.Deserialize (new XmlTextReader (file));
	}

	ArrayList bloggers;
	Hashtable idToBlogger;
	[XmlElement ("Blogger", typeof (Blogger))]
	public ArrayList Bloggers {
		get {
			return bloggers;
		}
		set {
			bloggers = value;
			bloggers.Sort (new BloggerComparer ());
			idToBlogger = new Hashtable ();
			foreach (Blogger b in bloggers)
				idToBlogger.Add (b.ID, b);
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
}

public class Blogger {
	[XmlAttribute] public string Name;
	[XmlAttribute] public string RssUrl;
	[XmlIgnore]
	public string ID {
		// Must look like an email to make rss happy
		get { return XmlConvert.EncodeLocalName (Name) + "@" + XmlConvert.EncodeLocalName (RssUrl); }
	}
		
	RssFeed feed;
	[XmlIgnore]
	public RssChannel Channel {
		get {
			if (feed == null)
				throw new Exception ("Must update feed before getting the channel");
			
			return feed.Channels [0];
		}
	}
	
	[XmlIgnore]
	public Uri HtmlUrl {
		get {
			return Channel.Link;
		}
	}
	
	public bool UpdateFeed ()
	{
		try {
		Console.WriteLine ("Getting {0}", RssUrl);
		if (feed == null) {
			feed = RssFeed.Read (RssUrl);
			foreach (RssItem i in feed.Channels [0].Items)
				i.Author = ID;
			return true;
		} else {
			RssFeed old = feed;
			feed = RssFeed.Read (feed);
			if (feed != old) {
				foreach (RssItem i in feed.Channels [0].Items)
					i.Author = ID;
				
				Console.WriteLine ("Updated {0}", Name);
			}
			return feed == old;
		}
		} catch (Exception e) {
			Console.WriteLine ("Exception from {0} : {1}", RssUrl, e);
			return false;
		}
	}
}
