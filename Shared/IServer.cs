using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IServer
    {
        PadInt CreatePadiInt(int txNumber, int uid);
        PadInt AccessPadiInt(int txNumber, int uid);
        bool TryWrite(int txNumber, PadInt padiInt);
        bool TxJoin(int txNumber);
        int TxBegin();
        bool TryTxCommit(int txNumber);
        bool CanCommit(int txNumber);
        bool TxCommit(int txNumber);
        bool TxAbort(int txNumber);
        string GetServerLocal(int id);
        string EnterRing(string local);
        bool Status();
        bool Fail();
        bool Freeze();
        bool Recover();
        bool Ping(int id);
        int[] Split();
        List<PadInt> GetSplitedObjects();
        List<PadInt> GetReplicatedList();
        bool ReplicateList(List<PadInt> padList, bool appendList);
        bool UpdateReplica(int uid, PadInt pInt);
    }
}
