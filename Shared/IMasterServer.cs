﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IMasterServer
    {

        Response CreatePadiInt(int txNumber, int uid);
        Response AccessPadiInt(int txNumber, int uid);
        Response TryWrite(int txNumber, PadiInt padiInt);
        int TxBegin();
        bool TxCommit(int txNumber);
        bool TxAbort(int txNumber);
        bool Status();
        bool Fail();
        bool Freeze();
        bool Recover();
        bool Register(int sid, string slocal);
        bool Unregister(int sid);

    }
}
