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
        private AppUI ui;

        public AppService(AppUI ui)
        {
            this.ui = ui;
        }

        void IClient.ChangeTargetServer(string url)
        {
            ui.Invoke(ui.sDelegate, url);
        }
    }
}
