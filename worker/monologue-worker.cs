using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Rss;

class MonologueWorker {
	static string bloggersFile;
	static string outputFile;
	static DateTime lastReadOfBloggersFile;
	
	
	static RssFeed outFeed;
	static BloggerCollection bloggers;
	
	static void Main (string [] args)
	{
		if (args.Length != 2)
			throw new Exception ("monologue-worker.exe BLOGGERS_FILE OUTPUT_FIILE");
		
		bloggersFile = args [0];
		outputFile = args [1];
		
		do {
			Console.WriteLine ("Updating");
			RunOnce ();
			System.Threading.Thread.Sleep (60 * 1000);
		} while (true);
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
		ch.Link = new Uri ("http://dunno.com");
		
		ArrayList stories = new ArrayList ();
		
		foreach (Blogger b in bloggers.Bloggers) {
			foreach (RssItem i in b.Channel.Items)
				stories.Add (i);
		}
		
		stories.Sort (new ReverseRssItemComparer ());
		
		foreach (RssItem itm in stories)
			ch.Items.Add (itm);
		
		outFeed.Channels.Add (ch);
		outFeed.Write (outputFile);
	}
	
	public class ReverseRssItemComparer : IComparer {
		int IComparer.Compare( Object x, Object y )
		{
			return DateTime.Compare (((RssItem)y).PubDate, ((RssItem)x).PubDate.Date);
		}
	}
}

public class BloggerCollection {
	static XmlSerializer serializer = new XmlSerializer (typeof (BloggerCollection));
	
	public static BloggerCollection LoadFromFile (string file)
	{
		return (BloggerCollection)serializer.Deserialize (new XmlTextReader (file));
	}
	
	[XmlElement ("Blogger", typeof (Blogger))]
	public ArrayList Bloggers;
}

public class Blogger {
	[XmlAttribute] public string Name;
	[XmlAttribute] public string Email;
	[XmlAttribute] public string RssUrl;
		
	RssFeed feed;
	[XmlIgnore]
	public RssChannel Channel {
		get {
			if (feed == null)
				throw new Exception ("Must update feed before getting the channel");
			
			return feed.Channels [0];
		}
	}
	
	public bool UpdateFeed ()
	{
		if (feed == null) {
			feed = RssFeed.Read (RssUrl);
			foreach (RssItem i in feed.Channels [0].Items)
				i.Author = Name;
			return true;
		} else {
			RssFeed old = feed;
			feed = RssFeed.Read (feed);
			if (feed != old) {
				foreach (RssItem i in feed.Channels [0].Items)
					i.Author = Name;
				
				Console.WriteLine ("Updated {0}", Name);
			}
			return feed == old;
		}
	}
}