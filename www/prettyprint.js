var keywords = "";

var keywordArray = new Array (
			"abstract","event","new","struct","as","explicit","null","switch","base","extern",
			"object","this","bool","false","operator","throw","break","finally","out","true",
			"byte","fixed","override","try","case","float","params","typeof","catch","for",
			"private","uint","char","foreach","protected","ulong","checked","goto","public",
			"unchecked","class","if","readonly","unsafe","const","implicit","ref","ushort",
			"continue","in","return","using","decimal","int","sbyte","virtual","default",
			"interface","sealed","volatile","delegate","internal","short","void","do","is",
			"sizeof","while","double","lock","stackalloc","else","long","static","enum",
			"namespace","string");


function paintColors ()
{
	keywords = "(";
	for (n=0; n<keywordArray.length; n++)
		keywords += "\\b" + keywordArray[n] + "\\b|";
	keywords += "string)";
	
	var elems = document.getElementsByTagName ("xmp");
	for (n=elems.length - 1; n>=0; n--)
	{
		if (elems[n].className == "code-csharp") {
			output = formatCs (elems [n].innerHTML);
			//elems [n].innerHTML = "";
			div = document.createElement("div");
			div.className = "code-csharp";
			div.innerHTML = output;
			elems [n].parentNode.replaceChild(div, elems [n]);
		}
		else if (elems[n].className == "code-xml") {
			output = formatXml (elems [n].innerHTML);
			//elems [n].innerHTML = "";
			div = document.createElement("div");
			div.className = "code-xml";
			div.innerHTML = output;
			elems [n].parentNode.replaceChild(div, elems [n]);
		}
	}
}

function formatCs (text)
{
	// remove trailing/leading lines
	while (text.charAt (0) == "\n" || text.charAt (0) == "\r" )
		text = text.substr (1);
	
	while (text.charAt (text.length) == "\n" || text.charAt (text.length) == "\r" )
		text = text.splice (0, -1);
		
	var re = new RegExp(" ","g");
	text = text.replace (re, "&nbsp;");

	re = new RegExp ("\"(((?!\").)*)\"","g");
	text = text.replace (re,"<span style='color:purple'>\"$1\"</span>");

	re = new RegExp ("//((.(?!\\\"\\<\\/span\\>))*)(\r|\n|\r\n)","g");
	text = text.replace (re,"<span style='color:green'>//$1</span><br/>");
	
	re = new RegExp (keywords,"g");
	text = text.replace (re,"<span style='color:blue'>$1</span>");
	
	re = new RegExp("\t","g");
	text = text.replace (re,"&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
	
	re = new RegExp("\n","g");
	text = text.replace (re,"<br/>");
	
	return text;
}

function formatXml (text)
{
	// remove trailing/leading lines
	while (text.charAt (0) == "\n" || text.charAt (0) == "\r" )
		text = text.substr (1);
	
	while (text.charAt (text.length) == "\n" || text.charAt (text.length) == "\r" )
		text = text.splice (0, -1);
	
	
	var re = new RegExp(" ", "g");
	text = text.replace (re, "&nbsp;");

	re = new RegExp("\t", "g");
	text = text.replace (re, "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

	var re = new RegExp("\r?\n", "g");
	text = text.replace (re, "_br_\n");
		
	var re = new RegExp ("<\\s*\\/([a-zA-Z-]+)(\\s[^>]*)?\\>","g");
	text = text.replace (re,"{blue:&lt;/}{maroon:$1}{blue:&gt;}");
	
	re = new RegExp ("<\\s*\\/([a-zA-Z-]+)(\\s[^>]*)?\\>","g");
	text = text.replace (re,"{blue:&lt;/}{maroon:$1}{blue:&gt;}");
	
	re = new RegExp ("<\\s*(((?!<\\>>).)*)\\s*\\>\\s*","g");
	text = text.replace (re,"{blue:&lt;}{maroon:$1}{blue:&gt;}");

	re = new RegExp ("<\\s*([a-zA-Z-]+)\\s*\\>\\s*","g");
	text = text.replace (re,"{blue:&lt;}{maroon:$1}{blue:&gt;}");
	
	re = new RegExp ("<\\s*(((?!<\\>>).)*)\\s*/\\s*\\>","g");
	text = text.replace (re,"{blue:&lt;}{maroon:$1}{blue:/&gt;}");
	
	re = new RegExp ("{(\\w*):([^}]*)}","g");
	text = text.replace (re,"<span style='color:$1'>$2</span>");

	re = new RegExp ("\"(((?![\"]).)*)\"","g");
	text = text.replace (re,"\"<span style='color:purple'>$1</span>\"");

	re = new RegExp("_br_", "g");
	text = text.replace (re, "<br/>");
	
	return text;
}