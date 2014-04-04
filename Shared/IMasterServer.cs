using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IMasterServer
    {

        PadInt CreatePadiInt(int uid);
        PadInt AccessPadiInt(int uid);
        bool TxBegin();
        bool TxCommit();
        bool TxAbort();
        bool Status();
        bool Fail();
        bool Freeze();
        bool Recover();
        bool Register(int sid, string slocal);
        bool Unregister(int sid);

    }
}
