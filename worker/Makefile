MCS=mcs /debug
MONO=mono 
monologue-worker.exe : monologue-worker.cs template4dotnet.cs
	$(MCS) /r:RSS.NET.dll /r:ICSharpCode.SharpZipLib.dll /out:$@ $^
	
run : monologue-worker.exe default.tpl
	$(MONO) $< bloggers.xml ../www/index.html ../www/index.rss

mig: monologue-worker.exe
	$(MONO) --debug $< miguel.xml ../www/index.html ../www/index.rss


push:
	scp bloggers.xml www.go-mono.com:monologue/worker/bloggers.xml
