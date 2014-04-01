using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    interface ISlaveServer
    {
        public PadInt CreatePadInt(int uid);
        public PadInt AccessPadInt(int uid);
        public bool TxBegin();
        public bool TxCommit();
        public bool TxAbort();
        public bool Status();
        public bool Fail();
        public bool Freeze();
        public bool Recover();
    }
}
