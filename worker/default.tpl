<html>
	<head>
		<title>Monologue::</title>
		<link rel='stylesheet' href='monologue.css' type='text/css' />
		<link rel='alternate' title='Monologue' href='index.rss' type='application/rss+xml' />
		<script src='prettyprint.js' type='text/javascript'>
		</script>
	</head>
	
	<body onload='paintColors();'>
		<h1>Monologue</h1>
		
		<div id='bloggers'>
			<h2>RSS</h2>
		        <a href="index.rss"><img src="xml.gif"></a> Monologue.
			
			<h2>Bloggers</h2>
			<ul>
				<!-- @@BLOGGER@@ -->
				<li><a href='@@BLOGGER_URL@@'>@@BLOGGER_NAME@@</a> <a href='@@BLOGGER_RSSURL@@'>(rss)</a></li>
				<!-- @@BLOGGER@@ -->
			</ul>
			
			<a href="http://www.go-mono.com"><img src="mono-powered-big.png" /></a>
		</div>
		
		<div id='blogs'>
			<!-- @@BLOG_DAY@@ -->
			<h2>@@DAY_DATE@@</h2>
			
				<!-- @@DAY_ENTRY@@ -->
				<h3><a href='@@ENTRY_LINK@@'>@@ENTRY_TITLE@@</a></h3>
				<div class='blogentry'>
					@@ENTRY_HTML@@
					<p>Posted at @@ENTRY_DATE@@</p>
				</div>
				<!-- @@DAY_ENTRY@@ -->
			<!-- @@BLOG_DAY@@ -->
		</div>
	</body>
</html>
