using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IMasterServer : IServer
    {
        bool Register(int sid, string slocal);
        bool Unregister(int sid);
        string GetAvailableServer();
    }
}
