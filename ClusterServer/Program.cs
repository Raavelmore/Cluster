using ClusterServer;

Thread mainThread = new(Server.Start);
mainThread.Start();

while (true) { }