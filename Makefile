worker :
	make -C worker
	
run : worker
	(cd www \
	mono ../monologue-worker.exe bloggers.xml monologue.rss & \
	xsp)