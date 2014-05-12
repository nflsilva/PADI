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
    public class PadInt
    {
        private int uid;
        private int value;
        private int version;
        private bool isDirty;


        public PadInt(int uid, int version)
        {
            this.uid = uid;
            this.value = 0;
            this.version = version;
            this.isDirty = false;
        }

        public bool isClean()
        {
            return !this.isDirty;
        }
        public void SetClean()
        {
            this.isDirty = false;
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
            this.isDirty = true;
        }
        public int GetVersion()
        {
            return this.version;
        }
    }
}
