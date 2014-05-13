using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Shared
{
    public interface IMasterServer : IServer
    {
        string[] Register(string slocal);
        bool Unregister(int sid);
        string GetAvailableServer();
        int GetTxNumber();
        bool RegisterNext(int sid, string nextLocal);
        string AddDeadServer(string deadLocal, string local);

        bool SplitRange(int uid, string local);
        int[] JoinRange(int uid, string local);
        string GetServerLocalForPadInt(int uid);
        string GetNextServer(string targLocal);
    }
}
