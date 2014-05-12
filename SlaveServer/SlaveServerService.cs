using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Threading;
using System.Threading.Tasks;

namespace SlaveServer
{

    class SlaveServerService : MarshalByRefObject, IServer
    {
        private Dictionary<int, PadInt> padiInts;               //this will hold the PadiIntObjects
        private Dictionary<int, string> servers;                //this will hold the servers locals;
        private Dictionary<int, List<PadInt>> transactions;     //this will hold the objects to be commited in this server in each transaction;
        private Dictionary<int, List<string>> participants;     //this will hold the participants in each transacions;
        private Dictionary<int, PadInt> replicas;               //this will hold the replicated objects from prev server;

        private int txNumber;

        private bool isWriting;     //flags for padiInts locks
        private bool isReading;
        private bool isRunning;     //state flag
        private bool fail;          //state flag for fail

        public bool pingRunning;

        private string nextServerLocal = "";
        private IServer nextServer = null;

        private int minUID;
        private int maxUID;

        private static SlaveUI ui;
        private Thread workerThread;


        public SlaveServerService(SlaveUI nui)
        {
            ui = nui;
            txNumber = 0;
            isWriting = false;
            isReading = false;
            isRunning = true;
            pingRunning = false;
            fail = false;
            padiInts = new Dictionary<int, PadInt>();
            servers = new Dictionary<int, string>();
            transactions = new Dictionary<int, List<PadInt>>();
            participants = new Dictionary<int, List<string>>();
            replicas = new Dictionary<int, PadInt>();

            minUID = 0;
            maxUID = 0;

            servers.Add(0, "tcp://localhost:2000/Server");

            workerThread = new Thread(PingNext);
            workerThread.Start();
        }


        #region replication

        List<PadInt> IServer.GetReplicatedList()
        {
            List<PadInt> list = new List<PadInt>();
            foreach (PadInt p in replicas.Values)
            {
                list.Add(p);
            }
            return list;
        }
        bool IServer.ReplicateList(List<PadInt> padList, bool appendList)
        {
            if (!appendList)
            {
                replicas.Clear();
            }
            foreach (PadInt p in padList)
            {
                replicas.Add(p.GetUid(), p);
            }
            return true;
        }
        bool IServer.UpdateReplica(int uid, PadInt pInt)
        {
            if (!replicas.ContainsKey(uid))
            {
                replicas.Add(uid, null);
            }
            replicas[uid] = pInt;
            return true;
        }

        public void AddPadInts(List<PadInt> padIntList)
        {
            foreach (PadInt p in padIntList)
            {
                padiInts.Add(p.GetUid(), p);
            }
        }

        #endregion

        #region ring
        List<PadInt> IServer.GetSplitedObjects()
        {
            List<PadInt> list = new List<PadInt>();
            List<int> toRemove = new List<int>();
            foreach (PadInt p in padiInts.Values)
            {
                if (p.GetUid() < minUID || p.GetUid() > maxUID)
                {
                    list.Add(p);
                    toRemove.Add(p.GetUid());
                }
            }
            foreach (int i in toRemove)
            {
                padiInts.Remove(i);
            }
            return list;
        }

        public void SetPadIntRange(int min, int max)
        {
            minUID = min;
            maxUID = max;
        }
        public void PingNext()
        {
            while (true)
            {
                if (pingRunning)
                {
                    try
                    {
                        nextServer = (IServer)Activator.GetObject(
                            typeof(IServer),
                            nextServerLocal);
                        nextServer.Ping(ui.GetServerId());
                    }
                    catch (Exception)
                    {
                        IMasterServer m = (IMasterServer)Activator.GetObject(
                            typeof(IMasterServer),
                            servers[0]);

                        int[] new_range = m.JoinRange(ui.GetServerId(), nextServerLocal);
                        minUID = new_range[0];
                        maxUID = new_range[1];

                        nextServerLocal = m.AddDeadServer(nextServerLocal, ui.GetServerLocal());

                        nextServer = (IServer)Activator.GetObject(
                            typeof(IServer),
                            nextServerLocal);

                        List<PadInt> toAdd = nextServer.GetReplicatedList();


                        List<PadInt> toReplicate = new List<PadInt>();
                        foreach (PadInt p in padiInts.Values)
                        {
                            toReplicate.Add(p);
                        }
                        nextServer.ReplicateList(toReplicate, true);

                        AddPadInts(toAdd);

                        ui.Invoke(ui.pDelegate, minUID, maxUID);
                        ui.Invoke(ui.cDelegate, "Server " + nextServerLocal + " doesn't responde. New next is: " + nextServerLocal);
                    }
                }
                Thread.Sleep(10000);
            }
        }

        public void SetNextServer(string local)
        {
            nextServerLocal = local;
            pingRunning = true;
        }


        public string GetNextServer()
        {
            return nextServerLocal;
        }
        string IServer.EnterRing(string local)
        {
            string response = nextServerLocal;
            SetNextServer(local + "/Server");
            IMasterServer m = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                servers[0]);
            m.RegisterNext(ui.GetServerId(), nextServerLocal);

            List<PadInt> toReplicate = new List<PadInt>();
            foreach (PadInt p in padiInts.Values)
            {
                if (p.GetUid() >= minUID && p.GetUid() <= maxUID)
                {
                    toReplicate.Add(p);
                }
            }
            IServer s = (IServer)Activator.GetObject(
                typeof(IServer),
                local + "/Server");
            s.ReplicateList(toReplicate, false);

            return response;
        }
        int[] IServer.Split()
        {
            int[] resp = new int[2];

            resp[1] = maxUID;
            maxUID = minUID + ((maxUID - minUID) / 2);
            resp[0] = maxUID + 1;
            ui.Invoke(ui.pDelegate, minUID, maxUID);

            return resp;
        }

        #endregion

        #region pad int

        PadInt IServer.CreatePadiInt(int txNumber, int uid)
        {
            CheckState();

            bool belogsHere = uid >= minUID && uid <= maxUID;
            if (belogsHere)
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

                IMasterServer m = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                servers[0]);
                string targetLocal = m.GetServerLocalForPadInt(uid);


                IServer s = (IServer)Activator.GetObject(
                    typeof(IServer),
                    targetLocal);

                return s.CreatePadiInt(txNumber, uid);
            }
        }
        PadInt IServer.AccessPadiInt(int txNumber, int uid)
        {
            CheckState();
            bool belogsHere = uid >= minUID && uid <= maxUID;
            if (belogsHere)
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
                IMasterServer m = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                servers[0]);
                string targetLocal = m.GetServerLocalForPadInt(uid);


                IServer s = (IServer)Activator.GetObject(
                    typeof(IServer),
                    targetLocal);

                return s.AccessPadiInt(txNumber, uid);
            }
        }

        bool IServer.TryWrite(int txNumber, PadInt padiInt)
        {
            CheckState();
            int uid = padiInt.GetUid();
            bool belogsHere = uid >= minUID && uid <= maxUID;
            if (belogsHere)
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

                IMasterServer m = (IMasterServer)Activator.GetObject(
                    typeof(IMasterServer),
                    servers[0]);
                string serverLocal = m.GetServerLocalForPadInt(uid);

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

        bool IServer.CanCommit(int txNumber)
        {
            return CanCommitMyself(txNumber);
        }

        private bool CanCommitMyself(int txNumber)
        {
            CheckState();
            GetReadLock();
            bool canCommit = true;
            foreach (PadInt pint in transactions[txNumber])
            {
                if (padiInts.ContainsKey(pint.GetUid()) &&
                    (pint.GetVersion() < padiInts[pint.GetUid()].GetVersion()))
                {
                    canCommit = false;
                    break;
                }
            }
            FreeReadLock();
            //Check timestamp logic
            return canCommit;
        }

        bool IServer.TryTxCommit(int txNumber)
        {
            CheckState();
            ui.Invoke(ui.cDelegate, "Coordinating Tx id: " + txNumber);
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

            //Commit Phase
            foreach (string local in participants[txNumber])
            {
                server = (IServer)Activator.GetObject(
                typeof(IServer),
                local);
                server.TxCommit(txNumber);

            }

            CommitTx(txNumber);
            //Commit on own state
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

                nextServer = (IServer)Activator.GetObject(
                    typeof(IServer),
                    nextServerLocal);
                nextServer.UpdateReplica(pint.GetUid(), pint);

                ui.Invoke(ui.intDelegate, new List<PadInt>(padiInts.Values));
                ui.Invoke(ui.repDelegate, new List<PadInt>(replicas.Values));

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

        int IServer.TxBegin()
        {
            CheckState();
            IMasterServer server = (IMasterServer)Activator.GetObject(
            typeof(IMasterServer),
            servers[0]);

            int txNumber = server.GetTxNumber();
            transactions.Add(txNumber, new List<PadInt>());
            participants.Add(txNumber, new List<string>());

            return txNumber;
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

        bool IServer.Ping(int nid)
        {
            ui.Invoke(ui.cDelegate, "Ping from server ID: " + nid.ToString());
            return true;
        }
        #endregion

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
                catch (SynchronizationLockException)
                {
                    ui.Invoke(ui.cDelegate, "SycExcception");
                }
                catch (ThreadInterruptedException)
                {
                    ui.Invoke(ui.cDelegate, "IntExcception");
                }
            }
            isWriting = true;
            ui.Invoke(ui.cDelegate, "WriteLock - ON");
        }
        private void FreeWriteLock()
        {
            isWriting = false;
            Monitor.Pulse(padiInts);
            Monitor.Exit(padiInts);
            ui.Invoke(ui.cDelegate, "WriteLock - OFF");
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
                catch (SynchronizationLockException)
                {
                    ui.Invoke(ui.cDelegate, "SycExcception");
                }
                catch (ThreadInterruptedException)
                {
                    ui.Invoke(ui.cDelegate, "IntExcception");
                }
            }
            isReading = true;
            ui.Invoke(ui.cDelegate, "ReadLock - ON");
        }

        private void FreeReadLock()
        {
            isReading = false;
            Monitor.Pulse(padiInts);
            Monitor.Exit(padiInts);
            ui.Invoke(ui.cDelegate, "ReadLock - OFF");
        }

        #endregion

    }
}
