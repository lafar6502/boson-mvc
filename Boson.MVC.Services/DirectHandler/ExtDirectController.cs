﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Web.Mvc;
using System.Web;
using System.Text;
using System.IO;
using Castle.MicroKernel;
using Castle.Windsor;
using Newtonsoft.Json;

namespace BosonMVC.Services.DirectHandler
{
    public class ExtDirectController : IController
    {
        public ExtDirectController()
        {
        }

        /// <summary>
        /// Service locator for finding direct handlers
        /// </summary>
        public IKernel ServiceLocator { get; set; }

        private Logger log = LogManager.GetCurrentClassLogger();

        public virtual void Execute(System.Web.Routing.RequestContext requestContext)
        {
            HttpContext context = HttpContext.Current;
            DirectHandlerUtils.ProcessRequest(context, ServiceLocator);
        }

       
    }
}
