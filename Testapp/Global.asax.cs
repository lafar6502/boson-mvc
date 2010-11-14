using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using BosonMVC.Services;

namespace Testapp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
            InitializeContainer();
            ControllerBuilder.Current.SetControllerFactory(new BosonMVC.Services.WindsorControllerFactory(Container));
            BosonMVC.Services.Boson.JSONViewFactory fact = new BosonMVC.Services.Boson.JSONViewFactory();
            fact.ServiceLocator = Container.Resolve<IServiceResolver>();
            fact.BaseDirectory = Server.MapPath("/");
            ViewEngines.Engines.Add(fact);
            WindsorControllerFactory.RegisterControllersFromAssembly(typeof(MvcApplication).Assembly, Container);
            WindsorControllerFactory.RegisterControllersFromAssembly(typeof(WindsorControllerFactory).Assembly, Container);
        }

        protected void InitializeContainer()
        {
            WindsorContainer wc = new WindsorContainer();
            wc.Register(Component.For<IServiceResolver>().ImplementedBy<WindsorServiceResolver>().LifeStyle.Singleton);
            Application.Add("container", wc);
        }

        public IWindsorContainer Container
        {
            get
            {
                return (IWindsorContainer)Application["container"];
            }
        }
    }
}