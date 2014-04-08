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


        public PadiInt(int uid){
            this.uid = uid;
            value = 0;
            version = 0;
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
