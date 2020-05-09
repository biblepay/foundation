using Saved.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace Saved
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Saved.Code.Common._pool = new Pool();
            if (Pool.fUseLocalXMR)
                Saved.Code.Common._xmrpool = new XMRPool();

            // Mission Critical
            if (!Debugger.IsAttached)
            {
                PoolCommon.fMonero2000 = false;
            }


        }
    }
}