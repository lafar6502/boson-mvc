using System;
using System.Web;
using NLog;
using Microsoft.Practices.ServiceLocation;
using System.Security.Principal;

namespace BosonMVC.Services
{
    public class AuthHttpModule : IHttpModule
    {
        private HttpApplication _ctx;
        private Logger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// You will need to configure this module in the web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            _ctx = context;
             context.PostAuthenticateRequest += new EventHandler(context_PostAuthenticateRequest);
        }

        private IServiceLocator GetServiceLocator()
        {
            return (IServiceLocator)_ctx.Application["servicelocator"];
        }

        void context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            IServiceLocator sl = GetServiceLocator();
            if (_ctx.User.Identity.IsAuthenticated)
            {
                IPrincipal pr = null;
                if (_ctx.Context.Session != null)
                    pr = _ctx.Context.Session["_BosonMVC_Principal"] as IPrincipal;
                
                if (pr == null)
                {
                    IAuthenticationService auth = sl.GetInstance<IAuthenticationService>();
                    pr = auth.GetAuthenticatedUser(_ctx.User.Identity.Name, _ctx.User.Identity.AuthenticationType);
                    if (pr != null && _ctx.Context.Session != null)
                        _ctx.Context.Session["_BosonMVC_Principal"] = pr;
                }
                if (pr != null)
                {
                    log.Debug("User authenticated: {0}", pr.Identity.Name);
                    System.Threading.Thread.CurrentPrincipal = pr;
                }
                else
                {
                    log.Warn("No principal for {0}", _ctx.User.Identity.Name);
                    _ctx.Response.Redirect("Unauthorized.html");
                }
            }

            
        }

   
        #endregion

    }
}
