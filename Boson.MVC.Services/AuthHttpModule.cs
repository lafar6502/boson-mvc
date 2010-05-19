using System;
using System.Web;
using NLog;
using System.Security.Principal;
using Castle.Windsor;

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
             context.PostAcquireRequestState += new EventHandler(context_PostAuthenticateRequest);
        }

        private IAuthenticationService GetAuthService()
        {
            IWindsorContainer wc = (IWindsorContainer) _ctx.Application["container"];
            if (wc == null)
            {
                log.Warn("Missing castle container - configure Application['container'] object");
                return null;
            }
            return wc.Resolve<IAuthenticationService>();
        }

        void context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            try
            {
                if (_ctx.User.Identity.IsAuthenticated)
                {
                    IPrincipal pr = null;
                    if (_ctx.Context.Session != null)
                        pr = _ctx.Context.Session["_BosonMVC_Principal"] as IPrincipal;

                    if (pr == null)
                    {
                        IAuthenticationService auth = GetAuthService();
                        if (auth == null) return; //do nothing
                        log.Debug("Creating user principal {0} for request {1}", _ctx.User.Identity.Name, _ctx.Request.Url);
                        pr = auth.GetAuthenticatedUser(_ctx.User.Identity.Name, _ctx.User.Identity.AuthenticationType);
                        if (pr != null && _ctx.Context.Session != null)
                            _ctx.Context.Session["_BosonMVC_Principal"] = pr;
                    }
                    if (pr != null)
                    {
                        log.Debug("User authenticated: {0} for request {1}", pr.Identity.Name, _ctx.Request.Url);
                        System.Threading.Thread.CurrentPrincipal = pr;
                    }
                    else
                    {
                        log.Warn("No principal for {0}", _ctx.User.Identity.Name);
                        _ctx.Response.Redirect("Unauthorized.html");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error authenticating request: {0}", ex);
                throw;
            }
        }

   
        #endregion

    }

    
}
