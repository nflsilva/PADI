using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IMasterServer
    {

        Response CreatePadiInt(int uid);
        Response AccessPadiInt(int uid);
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
