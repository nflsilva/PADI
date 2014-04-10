using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Shared
{
    [Serializable]
    public class PadiInt
    {
        private int uid;
        private int value;
        private int version;


        public PadiInt(int uid, int version)
        {
            this.uid = uid;
            this.value = 0;
            this.version = version;
        }

        public int GetUid()
        {
            return this.uid;
        }

        public int Read()
        {
            return this.value;
        }

        public void Write(int value)
        {
            this.value = value;
        }
        public int GetVersion()
        {
            return this.version;
        }
    }
}
