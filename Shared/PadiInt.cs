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
    public class PadiInt : MarshalByRefObject
    {
        private int uid;
        private int value;


        public PadiInt(int uid){
            uid = 0;
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
    }
}
