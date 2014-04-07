using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Threading.Tasks;

namespace SlaveServer
{

    class SlaveServerService : MarshalByRefObject, ISlaveServer
    {

        private static int MAX_NUM_SERVERS = 3;
        private static SlaveUI ui;
        private Dictionary<int, PadiInt> padiInts;
        private Dictionary<int, string> servers;                //this will hold the servers locals;
        private Dictionary<int, List<PadiInt>> transactions;    //this will hold the objects to be commited in this server in each transaction;
        private Dictionary<int, List<string>> participants;     //this will hold the participants in each transacions;


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

        Response ISlaveServer.CreatePadiInt(int txNumber, int uid)
        {
            if (!transactions.ContainsKey(txNumber))
            {
                transactions.Add(txNumber, new List<PadiInt>());
            }

            int targetServer = uid % MAX_NUM_SERVERS;
            if (targetServer == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Create PadiInt>  PadiInt id: " + uid.ToString() + " already exists!");
                    return new Response(false, null, null);
                }
                else
                {
                    PadiInt pint = new PadiInt(uid);
                    padiInts.Add(uid, pint);
                    ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " was created on transaction " + txNumber + " !");
                    return new Response(false, null, pint);
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");

                //HACK, need to be changed using something else
                if (targetServer == 0)
                {
                IMasterServer master = (IMasterServer)Activator.GetObject(
                    typeof(IMasterServer),
                    servers[targetServer]);
                return master.CreatePadiInt(txNumber, uid);
                }
                else
                {
                ISlaveServer slave = (ISlaveServer)Activator.GetObject(
                    typeof(ISlaveServer),
                    servers[targetServer]);
                return slave.CreatePadiInt(txNumber, uid);
                }

            }
        }

        Response ISlaveServer.AccessPadiInt(int txNumber, int uid)
        {
            int targetServer = uid % MAX_NUM_SERVERS;
            if (targetServer == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was requested.");
                    return new Response(false, null, padiInts[uid]);
                }
                else
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was not found.");
                    return new Response(false, null, null);
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");

                //HACK, need to be changed using something else
                if (targetServer == 0)
                {
                    IMasterServer master = (IMasterServer)Activator.GetObject(
                        typeof(IMasterServer),
                        servers[targetServer]);
                    return master.AccessPadiInt(txNumber, uid);
                }
                else
                {
                    ISlaveServer slave = (ISlaveServer)Activator.GetObject(
                        typeof(ISlaveServer),
                        servers[targetServer]);
                    return slave.AccessPadiInt(txNumber, uid);
                }
            }
        }

        Response ISlaveServer.TryWrite(int txNumber, PadiInt padiInt)
        {
            return null;
        }

        #endregion

        #region transactions
        bool ISlaveServer.JoinTx(int txNumber)
        {
            return true;
        }

        int ISlaveServer.TxBegin()
        {
            return 0;
        }

        bool ISlaveServer.TxCommit(int txNumber)
        {
            return false;
        }

        bool ISlaveServer.TxAbort(int txNumber)
        {
            return false;
        }

        #endregion

        #region nodes

        bool ISlaveServer.Status()
        {
            return true;
        }

        bool ISlaveServer.Fail()
        {
            return false;
        }


        bool ISlaveServer.Freeze()
        {
            return false;
        }

        bool ISlaveServer.Recover()
        {
            return false;
        }
        #endregion


    }
}
