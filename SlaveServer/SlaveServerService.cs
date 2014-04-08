using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Threading.Tasks;

namespace SlaveServer
{

    class SlaveServerService : MarshalByRefObject, IServer
    {

        private static int MAX_NUM_SERVERS = 3;

        private Dictionary<int, PadiInt> padiInts;              //this will hold the PadiIntObjects
        private Dictionary<int, string> servers;                //this will hold the servers locals;
        private Dictionary<int, List<PadiInt>> transactions;    //this will hold the objects to be commited in this server in each transaction;
        private Dictionary<int, List<string>> participants;     //this will hold the participants in each transacions;

        private int txNumber;

        private static SlaveUI ui;

        public SlaveServerService(SlaveUI nui)
        {
            ui = nui;
            padiInts = new Dictionary<int, PadiInt>();
            servers = new Dictionary<int, string>();
            transactions = new Dictionary<int, List<PadiInt>>();
            participants = new Dictionary<int, List<string>>();

            //DIRTY HACK
            servers.Add(0, "tcp://localhost:8086/MasterService");
            servers.Add(1, "tcp://localhost:8081/server-1");
            servers.Add(2, "tcp://localhost:8082/server-2");
        }

        #region pad int

        PadiInt IServer.CreatePadiInt(int txNumber, int uid)
        {
            int targetServerID = uid % MAX_NUM_SERVERS;
            if (targetServerID == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Create PadiInt>  PadiInt id: " + uid.ToString() + " already exists!");
                    return null;
                }
                else
                {
                    PadiInt pint = new PadiInt(uid);
                    ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " was created on transaction " + txNumber +" !");
                    return pint;
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                IServer server = (IServer)Activator.GetObject(
                typeof(IServer),
                servers[targetServerID]);
                return server.CreatePadiInt(txNumber, uid);
            }
        }

        //This function corresponds to a read(); the txNumber will be here only for future use, in case of a change, but for an
        //optimistic aprouch, this isn't used
        PadiInt IServer.AccessPadiInt(int txNumber, int uid)
        {
            int targetServerID = uid % MAX_NUM_SERVERS;
            if (targetServerID == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was requested.");
                    return padiInts[uid];
                }
                else
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was not found.");
                    return null;
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                IServer server = (IServer)Activator.GetObject(
                typeof(IServer),
                servers[targetServerID]);
                return server.AccessPadiInt(txNumber, uid);
            }
        }

        bool IServer.TryWrite(int txNumber, PadiInt padiInt)
        {
            int uid = padiInt.GetUid();
            int targetServerID = uid % MAX_NUM_SERVERS;

            if (targetServerID == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    transactions[txNumber].Add(padiInt);
                    ui.Invoke(ui.cDelegate, "Try> Write PadiInt id: " + uid.ToString() + "with value: " + padiInt.Read());
                    return true;
                }
                else
                {
                    ui.Invoke(ui.cDelegate, "Try> PadiInt id: " + uid.ToString() + " was not found. (this should never happen)");
                    return false;
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Try> PadiInt id: " + uid.ToString() + " isn't in this server.");
                string serverLocal = servers[targetServerID];
                IServer server = (IServer)Activator.GetObject(
                typeof(IServer),
                serverLocal);

                participants[txNumber].Add(serverLocal);

                return server.TryWrite(txNumber, padiInt);
            }
        }
           

        #endregion

        #region transactions
        bool IServer.JoinTx(int txNumber)
        {
            return true;
        }

        bool IServer.TryTxCommit(int txNumber)
        {
            //Coordenate Transaction
            return true;
        }

        int IServer.TxBegin()
        {
            return 0;
        }

        bool IServer.TxCommit(int txNumber)
        {
            return false;
        }

        bool IServer.TxAbort(int txNumber)
        {
            return false;
        }

        #endregion

        #region nodes

        bool IServer.Status()
        {
            return true;
        }

        bool IServer.Fail()
        {
            return false;
        }


        bool IServer.Freeze()
        {
            return false;
        }

        bool IServer.Recover()
        {
            return false;
        }
        #endregion


    }
}
