MCS=mcs /debug
MONO=mono 
monologue-worker.exe : monologue-worker.cs template4dotnet.cs
	$(MCS) /r:RSS.NET.dll /out:$@ $^
	
run : monologue-worker.exe default.tpl
	$(MONO) $< bloggers.xml ../www/index.html ../www/index.rss

mig: monologue-worker.exe
	$(MONO) $< miguel.xml ../www/index.html ../www/index.rss

