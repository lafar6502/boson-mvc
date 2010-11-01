using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Castle.Windsor;
using System.Reflection;
using Castle.MicroKernel.Registration;

namespace BosonMVC.Services
{
    public class WindsorControllerFactory : DefaultControllerFactory
    {
        private IWindsorContainer _wc;

        public WindsorControllerFactory(IWindsorContainer wc)
        {
            _wc = wc;
        }

        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null) return null;
            return (IController)_wc.Resolve(controllerType);
        }

        /* MVC 1.0
        protected override IController GetControllerInstance(Type controllerType)
        {
            return (IController)_wc.Resolve(controllerType);
        }*/

        public override void  ReleaseController(IController controller)
        {
            _wc.Release(controller);
        }

        public static void RegisterControllersFromAssembly(Assembly asm, IWindsorContainer wc)
        {
            foreach (Type t in asm.GetTypes())
            {
                if (typeof(IController).IsAssignableFrom(t))
                {
                    if (wc.Kernel.GetHandler(t) != null)
                    {
                        continue;
                    }
                    wc.Register(Component.For(typeof(IController), t).ImplementedBy(t).LifeStyle.Transient);
                }
            }
        }
    }
}
