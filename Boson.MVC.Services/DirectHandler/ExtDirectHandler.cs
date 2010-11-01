using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NLog;
using Newtonsoft.Json;
using Castle.MicroKernel;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Castle.Windsor;
using System.Web.SessionState;

namespace BosonMVC.Services.DirectHandler
{
    public class ExtDirectHandler : IHttpHandler, IRequiresSessionState
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        private Dictionary<string, Type> _actionTypes = new Dictionary<string, Type>();

        public IWindsorContainer ServiceLocator { get; set; }
        #region IHttpHandler Members


        public bool IsReusable
        {
            get { return true; }
        }

        
        public string APINamespace
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["BosonMVC.ExtDirectHandler.ApplicationNamespace"];
            }
        }

        [ThreadStatic]
        private static HttpContext _curCtx;

        public static HttpContext CurrentHttpContext
        {
            get { return _curCtx; }
        }

        public void ProcessRequest(HttpContext context)
        {
            
            ServiceLocator = (IWindsorContainer)context.Application["container"];
            try
            {
                _curCtx = context;
                DirectHandlerUtils.ProcessRequest(context, ServiceLocator.Kernel, APINamespace);
            }
            finally
            {
                _curCtx = null;
            }
        }

        #endregion
    }
}
