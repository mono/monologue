MCS=mcs -debug
MONO=mono

monologue-worker.exe : monologue-worker.cs template4dotnet.cs RSS.NET.dll
	$(MCS) /r:RSS.NET.dll /out:$@ monologue-worker.cs template4dotnet.cs

run : monologue-worker.exe default.tpl
	$(MONO) --debug $< bloggers.xml ../www/index.html ../www/index.rss --verbose

mig: monologue-worker.exe
	$(MONO) --debug $< miguel.xml ../www/index.html ../www/index.rss

push:
	scp bloggers.xml root@www.go-mono.com:/home/gonzalo/monologue/worker/bloggers.xml

clean:
	rm -f monologue-worker.exe RSS.NET.dll

RSS.NET.dll: 
	(cd ../RSS.NET; make && cp RSS.NET.dll ../worker)

