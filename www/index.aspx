<%@ Page Language="C#" %>
<%@ import namespace="Rss" %>
<html>
<head>
<script language="C#" runat="server">

	void Page_Load(object src, EventArgs e)
	{
		if (!Page.IsPostBack){
			DaysList.DataBind ();
		}

	}
			
	IList GetDaysCollectionList ()
	{
		Hashtable ht = new Hashtable ();
		
		foreach (RssItem itm in RssFeed.Read (Server.MapPath ("monologue.rss")).Channels [0].Items) {
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
	
	public class ReverseRssItemColComparer : IComparer {
		int IComparer.Compare( Object x, Object y )
		{
			return DateTime.Compare (((RssItem)((ArrayList)y) [0]).PubDate.Date, ((RssItem)((ArrayList)x) [0]).PubDate.Date);
		}
	}
</script>
	
	<title>Monologue::</title>
<style type="text/css">
h1 {
	color: #efefef;
	font-size: 14pt;
	font-family: "Trebuchet MS";
	border: 0;
	margin: 0;
	padding: 1em;
	background: #666666;
}

h2, h3, h4, h5, h6 {
	font-family: Verdana,sans-serif;
	font-weight: bold;
	margin: 9pt;
}

h2, h3 {
	font-size: 18px;
}

h2 {
	padding: 3px;
	color: #000000;
}

h3 {
	font-size: 13px;
	border-bottom: 2px solid #dddddd;
}
h3 a {
	color: black;
	text-decoration: none;
}

body, table {
	background-color: #ffffff;
	font-family: Verdana, sans-serif; font-size: 12px;
	color: black;
	margin: 0;
	padding: 0;
	border: 0;
}

.blogentry {
	margin-left: 2em;
	margin-right: 2em;
}

.blogentry h1, .blogentry h2, .blogentry h3 {
	padding: 0;
	margin: 0;
	border: 0;
}	

img {
	border: 0;
	vertical-align: top;
}
</style>
</head>
<body>

<h1>Monologue</h1>
	
<asp:Repeater id="DaysList" runat="server" DataSource="<%# GetDaysCollectionList () %>">
<ItemTemplate>
	<h2><%# ((RssItem)(((ArrayList)Container.DataItem) [0])).PubDate.Date.ToString ("M") %></h2>
	<asp:Repeater id="NewsItems" runat="server" DataSource="<%# Container.DataItem %>">
		<ItemTemplate>
		<h3><a href="<%#((RssItem)(Container.DataItem)).Link %>"><%# ((RssItem)(Container.DataItem)).Author %>: <%# ((RssItem)(Container.DataItem)).Title %></a></h3>
		<div class="blogentry">
		<%# ((RssItem)(Container.DataItem)).Description %>
		<div>Posted at <%# ((RssItem)(Container.DataItem)).PubDate.ToString ("t") %></div>
		</div>
		</ItemTemplate>
	</asp:Repeater>
</ItemTemplate>
</asp:Repeater>

</body>
</html>


