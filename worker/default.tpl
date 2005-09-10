<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
<title>Monologue - Voices of the Mono Project</title>
<link rel="stylesheet" href="css/monologue.css" type="text/css" />
<link rel="stylesheet" href="css/planet.css" type="text/css" />
<link rel="alternate" title="Monologue" href="index.rss" type="application/rss+xml" />
<!--[if IE]>
<link rel="stylesheet" href="css/ie.css" type="text/css" />
<![endif]-->
<script src="js/prettyprint.js" type="text/javascript"></script>
<script src="js/collapse.js" type="text/javascript"></script>

<body>
	<div id="header">
		<div id="top-right-links"><a href="http://www.mono-project.com/">Return to mono-project.com &raquo;</a></div>
		<div id="logo">&nbsp;</div>
		<h1>Monologue</h1>
	</div>
		
	<div id="blogs">
	
	<!-- @@BLOG_DAY@@ -->
	<h2 class="date">@@DAY_DATE@@</h2>
			
	<!-- @@DAY_ENTRY@@ -->
	<div class="entry">
		<div class="person-info">
		<a href="@@ENTRY_PERSON_URL@@">
			<img class="face" src="images/heads/@@ENTRY_PERSON_HEAD@@" alt="@@ENTRY_PERSON_IRCNICK@@"/>
			<br /><br />
			@@ENTRY_PERSON@@
			<br />@@ENTRY_PERSON_IRCNICK@@
		</a>
		</div>

		<div class="post">
		<div class="post2">
			<div class="post-header">
				<div class="expander"><input class="collapse-button" type="button" onClick="Collapse(this);" value="(Collapse)"></div>
				<h4 class="post-title"><a href="@@ENTRY_LINK@@">@@ENTRY_TITLE@@</a></h4>
			</div>
			
			<div class="post-contents">
@@ENTRY_HTML@@
			</div>
			
			<div class="post-footer">
				<p><a href="@@ENTRY_LINK@@">@@ENTRY_DATE@@</a>
			</div>
		</div>
		</div>
	</div>
	<!-- @@DAY_ENTRY@@ -->
	<!-- @@BLOG_DAY@@ -->
			
	<div id="bloggers">

	<h2>Monologue</h2>
		<p>Monologue is a window into the world, work, and lives of the community members and developers that make up the <a href="http://mono-project.com/">Mono Project</a>, which is a free cross-platform development environment used primarily on Linux.</p>
		<p>If you would rather follow Monologue using a newsreader, we provide the following feed:</p>
		<p><a href="index.rss"><img src="images/xml.gif"></a>  RSS 2.0 Feed</p>
			
		<h2>Bloggers</h2>
		<ul>
			<!-- @@BLOGGER@@ -->
			<li><div><img class="head" src="images/heads/@@BLOGGER_HEAD@@" alt="@@BLOGGER_IRCNICK@@" /></div>
				<a href="@@BLOGGER_URL@@">@@BLOGGER_NAME@@</a>
				<div>
					<a href="@@BLOGGER_RSSURL@@"><img src="images/feed.png" alt="@@BLOGGER_IRCNICK@@ feed"></a>
					<div class="ircnick">@@BLOGGER_IRCNICK@@</div>
				</div>
			</li><!-- @@BLOGGER@@ -->
		</ul>
			
		<a href="http://www.go-mono.com"><img src="images/mono-powered-big.png" /></a>
	</div>			

	</body>
</html>
