MCS=mcs /debug
MONO=mono --debug
monologue-worker.exe : monologue-worker.cs
	$(MCS) /r:RSS.NET.dll /out:$@ $^
	
run : monologue-worker.exe
	$(MONO) $< bloggers.xml ../www/index.html ../www/index.rss

mig: monologue-worker.exe
	$(MONO) $< miguel.xml ../www/index.html ../www/index.rss

