<html>
	<head>
		<title>Monologue::</title>
		<link rel='stylesheet' href='monologue.css' type='text/css' />
		<script src='prettyprint.js' type='text/javascript'>
		</script>
	</head>
	
	<body onload='paintColors();'>
		<h1>Monologue</h1>
		<div id='bloggers'>
			<h2>Bloggers</h2>
			<ul>
				<!-- @@BLOGGER@@ -->
				<li><a href='@@BLOGGER_URL@@'>@@BLOGGER_NAME@@</a> <a href='@@BLOGGER_RSSURL@@'>(rss)</a></li>
				<!-- @@BLOGGER@@ -->
			</ul>
		</div>
		
		<div id='blogs'>
			<!-- @@BLOG_DAY@@ -->
			<h2>@@DAY_DATE@@</h2>
			
				<!-- @@DAY_ENTRY@@ -->
				<h3><a href='@@ENTRY_LINK@@'>@@ENTRY_PERSON@@: @@ENTRY_TITLE@@</a></h3>
				<div class='blogentry'>
					@@ENTRY_HTML@@
					<p>Posted at @@ENTRY_DATE@@</p>
				</div>
				<!-- @@DAY_ENTRY@@ -->
			<!-- @@BLOG_DAY@@ -->
		</div>
	</body>
</html>