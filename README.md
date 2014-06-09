PADI
====

PADI-DSTM project

How to run:

1. Run a Master Server instance and start it;
2. Run as much Slave Servers and SampleClientApp as you like
3. Start transations. Create, Access and write PadInts into the system.
4. Commit transactions.

Note:
It's possible to run any application as a client of this system. To do so,nit's necessary to link your custom application with PADI-DSTM library and the Shared library.
To do this, first add the references for these two librarys on your solution.
Secondly, add the "using PADI-DSTM" and "using Shared" on the top of your code.

