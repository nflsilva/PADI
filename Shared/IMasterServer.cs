﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Shared
{
    public interface IMasterServer : IServer
    {
        int Register(string slocal);
        bool Unregister(int sid);
        string GetAvailableServer();
        int GetTxNumber();
    }
}
