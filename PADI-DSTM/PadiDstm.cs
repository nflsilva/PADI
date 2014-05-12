using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net.Sockets;
using Shared;

namespace PADI_DSTM
{
    public static class PadiDstm
    {

        private static int APP_DEFAULT_PORT = 9000;
        private static int MASTER_DEFAULT_PORT = 2000;
        private static string INTRO_MSG = "Hello, welcome to PADI-DSTM!";
        private static string MASTER_SERVER_LOCAL = "tcp://localhost:2000/Server";

        private static TcpChannel channel;
        private static IServer server;
        private static Dictionary<int, PadInt> cache;
        private static Dictionary<int, bool> dirty;
        private static int txNumber;


        public static PadInt CreatePadInt(int uid)
        {
            PadInt pint = null;
            try
            {
                pint = server.CreatePadiInt(txNumber, uid);
                pint.Write(pint.Read());
            }
            catch (Exception)
            {
                ConnectToSystem();
                CreatePadInt(uid);
            }

            if (pint != null)
            { 
                if (!cache.ContainsKey(pint.GetUid()))
                {
                    cache.Add(pint.GetUid(), pint);
                }

                if (!cache.ContainsKey(pint.GetUid()))
                {
                    dirty.Add(pint.GetUid(), true);
                }
                else {
                    dirty[pint.GetUid()] = true;
                }

            }
            return pint;
        }

        public static PadInt WritePadInt(int uid, int pint) 
        {
            if (cache.ContainsKey(uid))
            {
                cache[uid].Write(pint);
                dirty[uid] = true;
                return cache[uid];
            }
            else {
                throw new ArgumentNullException();
            }
        }

        public static PadInt AccessPadInt(int uid)
        {
            PadInt pint = null;
            if (cache.ContainsKey(uid))
            {
                pint = cache[uid];
            }
            else
            {
                try
                {
                    pint = server.AccessPadiInt(txNumber, uid);
                }
                catch (Exception)
                {
                    ConnectToSystem();
                    AccessPadInt(uid);
                }
            }

            if (pint != null)
            {
                if (cache.ContainsKey(pint.GetUid()))
                {
                    cache.Remove(pint.GetUid());
                }
                if (!cache.ContainsKey(pint.GetUid()))
                {
                    cache.Add(pint.GetUid(), pint);
                }

                if (!cache.ContainsKey(pint.GetUid()))
                {
                    dirty.Add(pint.GetUid(), false);
                }
                else
                {
                    dirty[pint.GetUid()] = false;
                }
            }
            return pint;
        }

        public static bool Init()
        {
            cache = new Dictionary<int, PadInt>();
            dirty = new Dictionary<int, bool>();
            Random rand = new Random();
            OpenChannel(APP_DEFAULT_PORT + rand.Next(1, 100));
            return true;
        }
        public static bool TxBegin()
        {

            try
            {
                txNumber = server.TxBegin();
            }
            catch (RemotingException)
            {
                ConnectToSystem();
                TxBegin();
            }

            
            if (txNumber == null)
            {
                throw new TxException("Couldn't start transaction");
            }
            return true;
        }

        public static bool TxCommit()
        {
            bool response = false;
            try
            {
                response = CommitChanges(txNumber);
            }
            catch (RemotingException)
            {
                ConnectToSystem();
                TxCommit();
            }

            dirty.Clear();
            cache.Clear();
            return response;


        }
        public static bool TxAbort()
        {

            bool response = false;
            try
            {
                response = server.TxAbort(txNumber);
            }
            catch (RemotingException)
            {
                ConnectToSystem();
                TxAbort();
            }

            dirty.Clear();
            cache.Clear();
            return response;

        }

        public static bool Status()
        {
            IServer s = (IServer)Activator.GetObject(
                typeof(IServer),
                MASTER_SERVER_LOCAL);
            return s.Status();
        }

        public static bool Fail(string URL)
        {
            IServer s = (IServer)Activator.GetObject(
                typeof(IServer),
                URL);
            return s.Fail();
        }

        public static bool Freeze(string URL)
        {
            IServer s = (IServer)Activator.GetObject(
                typeof(IServer),
                URL);
            return s.Freeze();
        }

        public static bool Recover(string URL)
        {
            IServer s = (IServer)Activator.GetObject(
                typeof(IServer),
                URL);
            return s.Recover();
        }


        private static bool OpenChannel(int port)
        {

            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            ConnectToSystem();

            return true;

        }
        private static bool CloseChannel()
        {
            ChannelServices.UnregisterChannel(channel);
            return true;
        }

        private static bool ConnectToSystem()
        {
            IMasterServer master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_SERVER_LOCAL);

            try
            {
                server = (IMasterServer)Activator.GetObject(
                    typeof(IMasterServer),
                    master.GetAvailableServer());
            }
            catch (RemotingException)
            {
                throw new TxException("Couldn't find system. Master must be down.");
            }


            return true;
        }

        private static bool CommitChanges(int txNumber)
        {
            bool tryResp = false;
            foreach (PadInt pint in cache.Values)
            {
                if (!pint.isClean())
                {
                    pint.SetClean();
                    tryResp = server.TryWrite(txNumber, pint);
                }
            }
            if (tryResp)
            {
                return server.TryTxCommit(txNumber);
            }

            return false;
        }
    }
}
