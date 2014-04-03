using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface ISlaveServer
    {
        PadInt CreatePadInt(int uid);
        PadInt AccessPadInt(int uid);
        bool TxBegin();
        bool TxCommit();
        bool TxAbort();
        bool Status();
        bool Fail();
        bool Freeze();
        bool Recover();
    }
}
