using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang;

namespace BosonMVC.Services.Boson
{
    public class QuackDictWrapper : IQuackFu
    {
        private IDictionary<string, object> _dic;

        public QuackDictWrapper(IDictionary<string, object> dic)
        {
            _dic = dic;
        }

        public QuackDictWrapper(System.Collections.IDictionary dic)
        {
            _dic = new Dictionary<string, object>();
            foreach (string k in dic.Keys)
            {
                _dic[k] = dic[k];
            }

        }

        public QuackDictWrapper()
        {
            _dic = new Dictionary<string, object>();
        }

        #region IQuackFu Members

        public object QuackGet(string name, object[] parameters)
        {
            object rv = null;
            _dic.TryGetValue(name, out rv);
            return rv;
        }

        public object QuackInvoke(string name, params object[] args)
        {
            throw new NotImplementedException();
        }

        public object QuackSet(string name, object[] parameters, object value)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> DataDictionary
        {
            get { return _dic; }
        }

        #endregion
    }
}
