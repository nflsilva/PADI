using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [Serializable]
    public class Response
    {
        private bool changeServer;
        private string local;

        private PadiInt pint;


        public Response(bool changeServer, string local, PadiInt pint)
        {
            this.changeServer = changeServer;
            this.local = local;
            this.pint = pint;
        }

        public bool IsChangeServer()
        {
            return changeServer;
        }

        public string GetLocal()
        {
            return local;
        }

        public PadiInt GetPadiInt()
        {
            return pint;
        }
    }
}
