using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel;

namespace BosonMVC.Services
{
    public class WindsorServiceResolver : IServiceResolver
    {
        private IKernel _krnl;
        public WindsorServiceResolver(IKernel kernel)
        {
            _krnl = kernel;
        }

        #region IServiceResolver Members

        

        public object GetInstance(Type t)
        {
            return _krnl.Resolve(t);
        }

        public object GetInstance(Type t, string name)
        {
            return _krnl.Resolve(name, t);
        }

        public T GetInstance<T>()
        {
            return _krnl.Resolve<T>();
        }

        public T GetInstance<T>(string name)
        {
            return _krnl.Resolve<T>(name);
        }

        #endregion

        #region IServiceResolver Members

        public ICollection<object> GetAllInstances(Type t)
        {
            Array a = _krnl.ResolveAll(t);
            return new List<object>(a.Cast<object>());
        }

        public ICollection<T> GetAllInstances<T>()
        {
            return _krnl.ResolveAll<T>();
        }

        #endregion
    }
}
