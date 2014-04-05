using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using System.Runtime.Remoting;

namespace SampleClientApp 
{
    class AppService : MarshalByRefObject, IClient
    {

        public AppService(AppUI ui)
        {

        }
    }
}
