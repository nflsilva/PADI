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
        private static int PING_DELAY = 2000;//ms

        private Dictionary<int, PadInt> padiInts;               //this will hold the PadiIntObjects
        private Dictionary<int, string> servers;                //this will hold the servers locals;
        private Dictionary<int, List<PadInt>> transactions;     //this will hold the objects to be commited in this server in each transaction;
        private Dictionary<int, List<string>> participants;     //this will hold the participants in each transacions;
        private Dictionary<int, string> deadServers;            //this will hold the dead servers on the ring, so the intervals stay balanced; 
        private Dictionary<int, string> pairs;                  //this will hold the pairs of each edge of the ring. Its used to recover the ring;
        private Dictionary<int, Tuple<int, int>> ranges;        //this will hold the range of each node of the ring. Used for routing;
        private Dictionary<int, PadInt> replicas;               //this will hold the replicated objects from prev server;


        private static MasterUI ui;
        private Thread workerThread;
        private int txNumber;
        private int nextAvailableID;
        private int nextAvailableServer;

        private bool isWriting;     //flags for padiInts locks
        private bool isReading;
        private bool isAccessingTxNumber;

        private bool isRunning;     //state flag
        private bool fail;          //state flag for fail

        public bool pingRunning;

        private string nextServerLocal = "";
        private IServer nextServer = null;

        private int minUID;
        private int maxUID;

        private int longerIntervalIndex = 0;
        private int expnent = 0;


        public MasterServerService(MasterUI nui)
        {
            ui = nui;
            minUID = 0;
            maxUID = 1000;
            txNumber = 0;
            nextAvailableServer = 0;
            nextAvailableID = 1;
            isWriting = false;
            isReading = false;
            isRunning = true;
            pingRunning = false;
            isAccessingTxNumber = false;
            fail = false;
            padiInts = new Dictionary<int, PadInt>();
            servers = new Dictionary<int, string>();
            transactions = new Dictionary<int, List<PadInt>>();
            participants = new Dictionary<int, List<string>>();
            deadServers = new Dictionary<int, string>();
            pairs = new Dictionary<int, string>();
            ranges = new Dictionary<int, Tuple<int, int>>();
            replicas = new Dictionary<int, PadInt>();
            ranges.Add(0, new Tuple<int, int>(minUID, maxUID));

            servers.Add(0, "tcp://localhost:2000/Server");
            SetNextServer(servers[0]);

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
        bool IMasterServer.SplitRange(int myID, string targLocal)
        {
            int targID = GetIDByLocal(targLocal);

            int new_max = ranges[targID].Item2;

            ranges[targID] = new Tuple<int, int> (ranges[targID].Item1, ranges[targID].Item1 + ((ranges[targID].Item2 - ranges[targID].Item1)/2));

            int new_min = ranges[targID].Item2 + 1;

            if (!ranges.ContainsKey(myID))
            {
                ranges.Add(myID, new Tuple<int, int>(new_min, new_max));
            }
            else
            {
                ranges[myID] = new Tuple<int, int>(new_min , new_max);
            }
            ui.Invoke(ui.cDelegate, "SPLITED> " + myID + " [" + ranges[myID].Item1 + "-" + ranges[myID].Item2 + "]");
            ui.Invoke(ui.cDelegate, "SPLITED> " + targID + " [" + ranges[targID].Item1 + "-" + ranges[targID].Item2 + "]");
            return true;
        }
        int[] IMasterServer.JoinRange(int myID, string targLocal)
        {
            int[] resp = new int[2];
            int targID = GetIDByLocal(targLocal);
            ui.Invoke(ui.cDelegate, "JOINED> " + myID + " with " + targID);

            //int new_min = 2 * ranges[targID].Item2 - 4 * ranges[myID].Item2;
            //FIXME: WRONG CALCULUM D:
            int new_min = ranges[myID].Item1;

            int new_max = ranges[targID].Item2;


            resp[0] = new_min;
            resp[1] = new_max;

            if (!ranges.ContainsKey(myID))
            {
                ranges.Add(myID, new Tuple<int, int>(new_min, new_max));
            }
            else
            {
                ranges[myID] = new Tuple<int, int>(new_min, new_max);
            }

            ui.Invoke(ui.cDelegate, "JOINED> " + myID + " [" + ranges[myID].Item1 + "-" + ranges[myID].Item2 + "]");
            return resp;
        }

        string IMasterServer.GetServerLocalForPadInt(int uid)
        {
            foreach (KeyValuePair<int, Tuple<int, int>> entry in ranges)
            {
                if (uid >= entry.Value.Item1 && uid <= entry.Value.Item2)
                {
                    return servers[entry.Key];
                }
            }
            return "NOT FOUND - SHOULD NEVER HAPPEN :O";
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

                        nextServerLocal = m.AddDeadServer(nextServerLocal, servers[0]);
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
                    Thread.Sleep(PING_DELAY);
                }
            }
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
        private string GetNodeWithLongerInterval()
        {
            string result;

            if (longerIntervalIndex == Math.Pow(2, expnent))
            {
                longerIntervalIndex = 0;
                expnent++;
            }
            result = servers[longerIntervalIndex];
            longerIntervalIndex++;

            return result;
        }
        public void SetNextServer(string local)
        {
            nextServerLocal = local;
            pingRunning = true;
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

        //This function corresponds to a read(); the txNumber will be here only for future use, in case of a change, but for an
        //optimistic aprouch, this isn't used
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
                string serverLocal =  m.GetServerLocalForPadInt(uid);


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
            ui.Invoke(ui.cDelegate, "Coordinating Tx id: " + txNumber);
            bool canCommit = true;
            IServer server;


            if (!CanCommitMyself(txNumber))
            {                
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
                AbortTx(txNumber);
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

            //Commit on own state
            CommitTx(txNumber);
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
                participants.Remove(txNumber);
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
        bool IServer.Ping(int nid)
        {
            ui.Invoke(ui.cDelegate, "Ping from " + nid.ToString());
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
            return txNumber++;
        }

        string IMasterServer.GetAvailableServer()
        {
            CheckState();
            string reps = servers[nextAvailableServer % servers.Count];
            nextAvailableServer++;
            return reps;
        }

        string[] IMasterServer.Register(string slocal)
        {
            CheckState();
            string[] response = new string[2];
            int sid;
            string next;

            if (deadServers.Count > 0)
            {
                sid = deadServers.Keys.Min();
                next = deadServers[sid];
                deadServers.Remove(sid);

            } else {
                sid = nextAvailableID++;
                next = GetNodeWithLongerInterval();
            }

            servers.Add(sid, slocal + "/Server");
            ui.Invoke(ui.cDelegate, "Registered server id: " + sid.ToString() + " located at: " + servers[sid]);
            response[0] = sid.ToString();
            response[1] = next;

            return response;
        }
        bool IMasterServer.Unregister(int sid)
        {
            CheckState();
            ui.Invoke(ui.cDelegate, "Removed server id: " + sid.ToString());
            return true;
        }

        string IMasterServer.AddDeadServer(string deadLocal, string local)
        {
            CheckState();
            int sid = GetIDByLocal(deadLocal);

            if (!deadServers.ContainsKey(sid))
            {
                deadServers.Add(sid, local);
            }
            servers.Remove(sid);
            return pairs[sid];

        }
        private int GetIDByLocal(string local)
        {
            foreach (KeyValuePair<int, string> entry in servers)
            {
                if (entry.Value.Equals(local))
                {
                    return entry.Key;
                }
            }
            return -1;
        }

        bool IMasterServer.RegisterNext(int sid, string nextLocal)
        {
            CheckState();
            if (pairs.ContainsKey(sid))
            {
                pairs.Remove(sid);
            }
            pairs.Add(sid, nextLocal);
            return true;
        }
        #endregion

    }
}
