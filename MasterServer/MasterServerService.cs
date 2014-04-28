using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer
{
    public class MasterServerService : MarshalByRefObject, IMasterServer
    {

        private static int MAX_NUM_SERVERS = 3;



        private Dictionary<int, PadInt> padiInts;              //this will hold the PadiIntObjects
        private Dictionary<int, string> servers;                //this will hold the servers locals;
        private Dictionary<int, List<PadInt>> transactions;    //this will hold the objects to be commited in this server in each transaction;
        private Dictionary<int, List<string>> participants;     //this will hold the participants in each transacions;

        private static MasterUI ui;
        private int txNumber;

        private int nextAvailableServer;

        private bool isWriting;     //flags for padiInts locks
        private bool isReading;
        private bool isAccessingTxNumber;

        private bool isRunning;     //state flag
        private bool fail;          //state flag for fail

        private object boolLock = new object(); //DIRTY HACK! Used to avoid active wait

        public MasterServerService(MasterUI nui)
        {
            ui = nui;
            txNumber = 0;
            nextAvailableServer = 0;
            isWriting = false;
            isReading = false;
            isRunning = true;
            isAccessingTxNumber = false;
            fail = false;
            padiInts = new Dictionary<int, PadInt>();
            servers = new Dictionary<int, string>();
            transactions = new Dictionary<int, List<PadInt>>();
            participants = new Dictionary<int, List<string>>();
            servers.Add(0, "tcp://localhost:8086/MasterService");
        }


        #region locks

        private void CheckState()
        {
            if (fail)
            {
                Environment.Exit(-1); //Dont know what to call here
            }
            else
            {
                while (!isRunning) ;
            }
        }
        private void GetWriteLock()
        {
            Monitor.Enter(padiInts);

            if (isWriting || isReading)
            {
                try
                {
                    Monitor.Wait(padiInts);
                }
                catch (SynchronizationLockException e)
                {
                    ui.Invoke(ui.cDelegate, "SycExcception");
                }
                catch (ThreadInterruptedException e)
                {
                    ui.Invoke(ui.cDelegate, "IntExcception");
                }
            }
            isWriting = true;
        }
        private void FreeWriteLock()
        {
            isWriting = false;
            Monitor.Pulse(padiInts);

            Monitor.Exit(padiInts);
        }

        private void GetReadLock()
        {
            Monitor.Enter(padiInts);

            if (isWriting)
            {
                try
                {
                    Monitor.Wait(padiInts);
                }
                catch (SynchronizationLockException e)
                {
                    ui.Invoke(ui.cDelegate, "SycExcception");
                }
                catch (ThreadInterruptedException e)
                {
                    ui.Invoke(ui.cDelegate, "IntExcception");
                }
            }
            isReading = true;
        }

        private void FreeReadLock()
        {
            isReading = false;
            Monitor.Pulse(padiInts);

            Monitor.Exit(padiInts);
        }

        #endregion

        #region pad int
        PadInt IServer.CreatePadiInt(int txNumber, int uid)
        {
            CheckState();

            int targetServerID = uid % MAX_NUM_SERVERS;
            if (targetServerID == ui.GetServerId())
            {
                GetReadLock();

                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Create PadiInt>  PadiInt id: " + uid.ToString() + " already exists!");
                    FreeReadLock();
                    return null;
                }
                else
                {
                    PadInt pint = new PadInt(uid, 0);
                    ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " was created on transaction " + txNumber + " !");
                    FreeReadLock();
                    return pint;
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                IServer server = (IServer)Activator.GetObject(
                typeof(IServer),
                GetServerById(targetServerID));
                return server.CreatePadiInt(txNumber, uid);
            }
        }

        //This function corresponds to a read(); the txNumber will be here only for future use, in case of a change, but for an
        //optimistic aprouch, this isn't used
        PadInt IServer.AccessPadiInt(int txNumber, int uid)
        {
            CheckState();

            int targetServerID = uid % MAX_NUM_SERVERS;
            if (targetServerID == ui.GetServerId())
            {
                GetReadLock();

                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was requested.");
                    PadInt pint = padiInts[uid];
                    FreeReadLock();
                    return pint;
                }
                else
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was not found.");
                    FreeReadLock();
                    return null;
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                IServer server = (IServer)Activator.GetObject(
                typeof(IServer),
                GetServerById(targetServerID));
                return server.AccessPadiInt(txNumber, uid);
            }
        }

        bool IServer.TryWrite(int txNumber, PadInt padiInt)
        {
            CheckState();

            int uid = padiInt.GetUid();
            int targetServerID = uid % MAX_NUM_SERVERS;

            if (targetServerID == ui.GetServerId())
            {
                if (transactions[txNumber].Contains(padiInt))
                {
                    transactions[txNumber].Remove(padiInt);
                }
                transactions[txNumber].Add(padiInt);
                ui.Invoke(ui.cDelegate, "Try> Write PadiInt id: " + uid.ToString() + "with value: " + padiInt.Read());
                return true;
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Try> PadiInt id: " + uid.ToString() + " isn't in this server.");
                string serverLocal = GetServerById(targetServerID);
                IServer server = (IServer)Activator.GetObject(
                typeof(IServer),
                serverLocal);


                if (!participants[txNumber].Contains(serverLocal))
                {
                    participants[txNumber].Add(serverLocal);
                    server.TxJoin(txNumber);
                }


                return server.TryWrite(txNumber, padiInt);
            }
        }

        #endregion

        #region transactions
        bool IServer.TxJoin(int txNumber)
        {
            CheckState();

            transactions.Add(txNumber, new List<PadInt>());
            return true;
        }

        int IServer.TxBegin()
        {
            CheckState();

            transactions.Add(txNumber, new List<PadInt>());
            participants.Add(txNumber, new List<string>());
            return txNumber++;
        }

        bool IServer.TryTxCommit(int txNumber)
        {
            CheckState();
            bool canCommit = true;
            IServer server;


            if (!CanCommitMyself(txNumber))
            {                
                AbortTx(txNumber);
                canCommit = false;
            }
            else
            {
                //Voting Phase
                foreach (string local in participants[txNumber])
                {
                    server = (IServer)Activator.GetObject(
                    typeof(IServer),
                    local);
                    if (!server.CanCommit(txNumber))
                    {
                        canCommit = false;
                        break;
                    }
                }

            }

            if (!canCommit)
            {

                foreach (string local in participants[txNumber])
                {
                    server = (IServer)Activator.GetObject(
                    typeof(IServer),
                    local);
                    server.TxAbort(txNumber);
                }
                return false;
            }
            CommitTx(txNumber);
            //Commit Phase
            foreach (string local in participants[txNumber])
            {
                server = (IServer)Activator.GetObject(
                typeof(IServer),
                local);
                server.TxCommit(txNumber);

            }

            //Commit on own state
            return true;
        }

        bool IServer.CanCommit(int txNumber)
        {
            return CanCommitMyself(txNumber);
        }

        private bool CanCommitMyself(int txNumber)
        {
            CheckState();
            GetReadLock();
            foreach (PadInt pint in transactions[txNumber])
            {
                if (padiInts.ContainsKey(pint.GetUid()) &&
                    (pint.GetVersion() < padiInts[pint.GetUid()].GetVersion()))
                {
                    return false;
                }
            }
            FreeReadLock();
            //Check timestamp logic
            return true;
        }

        private bool CommitTx(int txNumber)
        {
            PadInt npint;
            GetWriteLock();
            foreach (PadInt pint in transactions[txNumber])
            {
                if (padiInts.ContainsKey(pint.GetUid()))
                {
                    padiInts.Remove(pint.GetUid());
                }
                npint = new PadInt(pint.GetUid(), pint.GetVersion() + 1);
                npint.Write(pint.Read());
                padiInts.Add(pint.GetUid(), npint);
            }
            FreeWriteLock();
            transactions.Remove(txNumber);
            participants.Remove(txNumber);
            ui.Invoke(ui.cDelegate, "TxCommit> Tx id: " + txNumber + " has been commited!");
            return true;
        }

        private bool AbortTx(int txNumber)
        {
          CheckState();
          if(transactions.ContainsKey(txNumber)){
                transactions.Remove(txNumber);
            }
            ui.Invoke(ui.cDelegate, "TxAbort> Tx id: " + txNumber + " has been aborted!");
            return true;
        }

        bool IServer.TxCommit(int txNumber)
        {
            CheckState();
            return CommitTx(txNumber);
        }

        bool IServer.TxAbort(int txNumber)
        {
            CheckState();
            return AbortTx(txNumber);
        }

        #endregion

        #region nodes
        string IServer.GetServerLocal(int id)
        {
            CheckState();
            if (servers.ContainsKey(id))
            {
                return servers[id];
            }
            else
            {
                return null;
            }
        }
        private string GetServerById(int id)
        {
            CheckState();
            if (servers.ContainsKey(id))
            {
                return servers[id];
            }
            else
            {
                //in the future, a new search function may be used
                IServer server = (IServer)Activator.GetObject(typeof(IServer), servers[0]);
                string local = server.GetServerLocal(id);
                servers.Add(id, local);
                return local;
            }
        }
        bool IServer.Status()
        {
            string state;
            if (isRunning)
            {
                state = "running:)";
            }
            else
            {
                state = "fozen:(";
            }

            ui.Invoke(ui.cDelegate, "Status> My current state is " + state);
            IServer s;
            foreach (KeyValuePair<int, string> pair in servers)
            {
                if (pair.Key == 0)
                {
                    continue;
                }
                s = (IServer)Activator.GetObject(typeof(IServer), pair.Value);

                s.Status();
            }

            return true;

        }

        bool IServer.Fail()
        {
            CheckState();
            fail = true;
            return true;
        }


        bool IServer.Freeze()
        {
            isRunning = false;
            return true;
        }

        bool IServer.Recover()
        {
            isRunning = true;
            fail = false;
            return true;
        }

        #endregion

        #region master
        int IMasterServer.GetTxNumber()
        {
            CheckState();
            /*
            int returnTxNumber = 0;
            Monitor.Enter(txNumber);
            {
                if (isAccessingTxNumber)
                {
                    try
                    {
                        Monitor.Wait(txNumber);
                    }
                    catch (SynchronizationLockException e)
                    {
                        ui.Invoke(ui.cDelegate, "SycExcception");
                    }
                    catch (ThreadInterruptedException e)
                    {
                        ui.Invoke(ui.cDelegate, "IntExcception");
                    }
                }
                isAccessingTxNumber = true;
                returnTxNumber = txNumber++;
                isAccessingTxNumber = false;
            }
            Monitor.Exit(txNumber);

            return returnTxNumber;*/
            return txNumber++;
        }

        string IMasterServer.GetAvailableServer()
        {
            CheckState();
            string reps = servers[nextAvailableServer % MAX_NUM_SERVERS];
            nextAvailableServer++;
            return reps;
        }

        bool IMasterServer.Register(int sid, string slocal)
        {
            CheckState();
            if (servers.ContainsKey(sid))
            {
                return false;
            }
            servers.Add(sid, slocal);
            ui.Invoke(ui.cDelegate, "Registered server id: " + sid.ToString() + " located at: " + slocal);
            return true;
        }
        bool IMasterServer.Unregister(int sid)
        {
            CheckState();
            if (servers.Remove(sid))
            {
                ui.Invoke(ui.cDelegate, "Removed server id: " + sid.ToString());
                return true;
            }
            return false;
        }
        #endregion

    }
}
