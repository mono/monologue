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
		if (args.Length != 3)
			throw new Exception ("monologue-worker.exe BLOGGERS_FILE HTML_OUTPUT RSS_OUTPUT");
		
		bloggersFile = args [0];
		outputFile = args [1];
		rssOutFile = args [2];

		RunOnce ();
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
		ch.Generator = "Monologue worker: b-diddy powerd";
		ch.Description = "The voices of Mono";
		ch.Link = new Uri ("http://monologue.go-mono.com");
		
		ArrayList stories = new ArrayList ();
		
		DateTime minPubDate = DateTime.Now.AddDays (-14);
		foreach (Blogger b in bloggers.Bloggers) {
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
				<script src='prettyprint.js' />
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
				", itm.Link, bloggers [itm.Author].Name, itm.Title, itm.Description, itm.PubDate.ToString ("h:m tt"));
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
	Hashtable emailToBlogger;
	[XmlElement ("Blogger", typeof (Blogger))]
	public ArrayList Bloggers {
		get {
			return bloggers;
		}
		set {
			bloggers = value;
			bloggers.Sort (new BloggerComparer ());
			emailToBlogger = new Hashtable ();
			foreach (Blogger b in bloggers) {
				emailToBlogger.Add (b.Email, b);
			}
		}
	}
	
	public Blogger this [string email] {
		get {
			return (Blogger)emailToBlogger [email];
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
	[XmlAttribute] public string Email;
		
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
		Console.WriteLine ("Getting {0}", RssUrl);
		if (feed == null) {
			feed = RssFeed.Read (RssUrl);
			foreach (RssItem i in feed.Channels [0].Items)
				i.Author = Email;
			return true;
		} else {
			RssFeed old = feed;
			feed = RssFeed.Read (feed);
			if (feed != old) {
				foreach (RssItem i in feed.Channels [0].Items)
					i.Author = Email;
				
				Console.WriteLine ("Updated {0}", Name);
			}
			return feed == old;
		}
	}
}