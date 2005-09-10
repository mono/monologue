function Collapse (e)
{
	for (var i = 0; i < e.parentNode.parentNode.parentNode.childNodes.length; i++) {
		var node = e.parentNode.parentNode.parentNode.childNodes [i];

		if (node.className == "post-contents") {
			if (node.style.display == "none") {
				node.style.display = "block";
				e.value = "(Collapse)";
			} else {
				node.style.display = "none";
				e.value = "(Expand)";
			}
			return;
		}
	}
}

function ExpandAll ()
{
	var entries = document.getElementById ("blogs");

	var result = evaluateXPath (entries, "//html/body/div[@id='blogs']/div[@class='entry']/div[@class='post']/div[@class='post2']/div[@class='post-contents']");
	var result1 = evaluateXPath (entries, "//html/body/div[@id='blogs']/div[@class='entry']/div[@class='post']/div[@class='post2']/div[@class='post-header']/div[@class='expander']/input");

	for (var i = 0; i < result.length; i++) {
		var node = result [i];
		
		node.style.display = 'block';
		result1[i].value = "(Collapse)";
	}

	var collapseAllLink = document.getElementById ("collapse-all");
	collapseAllLink.innerHTML = "If you would like, you can <a href='javascript:CollapseAll()'>collapse all of the posts on this page</a> and view only headlines.";
}
function CollapseAll ()
{
	var entries = document.getElementById ("blogs");

	var result = evaluateXPath (entries, "//html/body/div[@id='blogs']/div[@class='entry']/div[@class='post']/div[@class='post2']/div[@class='post-contents']");
	var result1 = evaluateXPath (entries, "//html/body/div[@id='blogs']/div[@class='entry']/div[@class='post']/div[@class='post2']/div[@class='post-header']/div[@class='expander']/input");

	for (var i = 0; i < result.length; i++) {
		var node = result [i];
		
		node.style.display = 'none';	
		result1 [i].value = "(Expand)";
	}

	var collapseAllLink = document.getElementById ("collapse-all");
	collapseAllLink.innerHTML = "If you would like, you can <a href='javascript:ExpandAll()'>expand all of the posts on this page</a> and view both headlines and the post text.";
}

// Evaluate an XPath expression aExpression against a given DOM node
// or Document object (aNode), returning the results as an array
// thanks wanderingstan at morethanwarm dot mail dot com for the
// initial work.
function evaluateXPath(aNode, aExpr) {
  var xpe = new XPathEvaluator();
  var nsResolver = xpe.createNSResolver(aNode.ownerDocument == null ?
    aNode.documentElement : aNode.ownerDocument.documentElement);
  var result = xpe.evaluate(aExpr, aNode, nsResolver, 0, null);
  var found = [];
  while (res = result.iterateNext())
    found.push(res);
  return found;
}

