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
        private static IMasterServer master;
        private Dictionary<int, PadiInt> padiInts = new Dictionary<int, PadiInt>();
        private Dictionary<int, string> servers = new Dictionary<int, string>();


        public SlaveServerService(SlaveUI nui)
        {
            ui = nui;
            //HACK
            servers.Add(0, "tcp://localhost:8086/MasterService");
            servers.Add(1, "tcp://localhost:8081/server-1");
            servers.Add(2, "tcp://localhost:8082/server-2");
        }

        #region pad int

        Response ISlaveServer.CreatePadiInt(int uid)
        {
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
                    ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " was created!");
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
                return master.CreatePadiInt(uid);
                }
                else
                {
                ISlaveServer slave = (ISlaveServer)Activator.GetObject(
                    typeof(ISlaveServer),
                    servers[targetServer]); 
                return slave.CreatePadiInt(uid);
                }

            }
        }

        Response ISlaveServer.AccessPadiInt(int uid)
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
                    return master.AccessPadiInt(uid);
                }
                else
                {
                    ISlaveServer slave = (ISlaveServer)Activator.GetObject(
                        typeof(ISlaveServer),
                        servers[targetServer]);
                    return slave.AccessPadiInt(uid);
                }
            }
        }

        #endregion

        #region transactions
        bool ISlaveServer.TxBegin()
        {
            return false;
        }

        bool ISlaveServer.TxCommit()
        {
            return false;
        }

        bool ISlaveServer.TxAbort()
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
