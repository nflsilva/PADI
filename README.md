PADI
====

PADI-DSTM project

How to run:

1. Run a Master Server instance and start it;
2. Run as much Slave Servers and SampleClientApp as you like
3. Start transations. Create, Access and write PadInts into the system.
4. Commit transactions.

Bugs/Hacks that need revision:
1. When a server fails, something the resulting slaves PadInt interval becomes buggy, need to fix that
2. Add TimeOuts on Access, Create, 2PC, etc, so the ring doesn't stop when a given server is froozen or down
3. Add Lock to TxBegin on master 
4. Add TxExceptions to PadiDstm and fix the "ghost" server bug

