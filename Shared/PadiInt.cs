using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PadiInt
    {
        private static int uid;
        private int value;


        public PadiInt(int uid){
            uid = 0;
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
