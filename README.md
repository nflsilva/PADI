PADI
====

PADI-DSTM project

How to run:

1. Run a Master Server instance and start it;
2. Run 2 Slave Servers intances adjust the IDs and the PORTs and start them;
3. If everything went well, you should see that each Slave server registered it self on Master;
4. Run a SampleClientApp and connect to the system;
5. Create PadiInts - Onlt createPadiInts works for all the 3 servers.
6. ???
7. Profit!

Bugs/Hacks that need revision:
1. Read settings from file;
2. SlaveServerService 
  2.1 How to get the other servers URL
3. IMasterServer, ISlaveServer - these interfaces should be the same, so the client/slave doesn't need to change Interface to comunicate with diferent servers that do the same! There should be a IServer as "SuperInterface" for those two.

