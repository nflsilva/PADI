﻿using System;
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

        private static int APP_DEFAULT_PORT = 8090;
        private static int MASTER_DEFAULT_PORT = 8086;
        private static string INTRO_MSG = "Hello, welcome to PADI-DSTM!";
        private static string APP_SERVER_NAME = "ClientService";
        private static string MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/MasterService";
        private static string APP_SERVER_LOCAL = "tcp://localhost:" + APP_DEFAULT_PORT.ToString() + "/" + APP_SERVER_NAME;

        private static TcpChannel channel;
        private static IServer server;
        private static Dictionary<int, PadInt> cache;
        private static Dictionary<int, bool> dirty;
        private static int txNumber;



        public static PadInt CreatePadInt(int uid)
        {
            PadInt pint = server.CreatePadiInt(txNumber, uid);
            if (pint != null)
            { 
                //these two lines may lead to crash IF two creates are done one after another
                cache.Add(pint.GetUid(), pint);
                dirty.Add(pint.GetUid(), true);
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
            PadInt pint;
            if (cache.ContainsKey(uid))
            {
                pint = cache[uid];
            }
            else
            {
                pint = server.AccessPadiInt(txNumber, uid);
            }

            if (pint != null)
            {
                if (cache.ContainsKey(pint.GetUid()))
                {
                    cache.Remove(pint.GetUid());
                }
                //these two lines may lead to crash IF two access are done one after another
                cache.Add(pint.GetUid(), pint);
                dirty.Add(pint.GetUid(), false);
            }
            return pint;
        }

        public static bool Init()
        {
            cache = new Dictionary<int, PadInt>();
            dirty = new Dictionary<int, bool>();
            try
            {
                return OpenChannel(APP_DEFAULT_PORT);
            }
            catch (SocketException) { 
                Random rand = new Random();
                return OpenChannel(APP_DEFAULT_PORT + rand.Next(1, 10));            
            }
        }

        public static bool Init(int port)
        {
            cache = new Dictionary<int, PadInt>();
            dirty = new Dictionary<int, bool>();

            return OpenChannel(port);
        }

        public static bool TxBegin()
        {
            txNumber = server.TxBegin();
            if (txNumber == null)
            {
                throw new TxException("Couldn't start transaction");
            }
            return true;
        }

        public static bool TxCommit()
        {
            if (CommitChanges(txNumber))
            {
                dirty.Clear();
                cache.Clear();
                return true;
            }
            //else
            //{
            //    throw new TxException("Couldn't commit transaction");
            //}
            return false;


        }
        public static bool TxAbort()
        {

            if (server.TxAbort(txNumber))
            {
                dirty.Clear();
                cache.Clear();
                return true;
            }
            else
            {
                throw new TxException("Couldn't abort transaction");
            }

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
            ChannelServices.RegisterChannel(channel, true);
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

            MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/MasterService";
            IMasterServer master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_SERVER_LOCAL);


            server = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                master.GetAvailableServer());

            return true;
        }

        private static bool CommitChanges(int txNumber)
        {
            bool tryResp = false;
            foreach (PadInt pint in cache.Values)
            {
                if (dirty[pint.GetUid()])
                {
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
