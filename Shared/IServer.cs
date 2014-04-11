﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IServer
    {
        PadiInt CreatePadiInt(int txNumber, int uid);
        PadiInt AccessPadiInt(int txNumber, int uid);
        bool TryWrite(int txNumber, PadiInt padiInt);
        bool TxJoin(int txNumber);
        int TxBegin();
        bool TryTxCommit(int txNumber);
        bool CanCommit(int txNumber);
        bool TxCommit(int txNumber);
        bool TxAbort(int txNumber);
        string GetServerLocal(int id);
        bool Status();
        bool Fail();
        bool Freeze();
        bool Recover();
    }
}