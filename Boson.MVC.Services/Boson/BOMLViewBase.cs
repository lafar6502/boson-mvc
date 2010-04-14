using System;
using System.Collections;
using System.Text;
using System.Web.Mvc;
using NLog;
using Boo.Lang;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;

namespace BosonMVC.Services.Boson
{
    public abstract class BOMLViewBase : IView
    {
        protected abstract void DoRender();
        protected ViewContext Context;
        protected Logger log = LogManager.GetCurrentClassLogger();
        
        public BOMLViewBase()
        {
            _viewDataWrapper = new Quacker(delegate(string name, object[] prm)
            {
                return Context.ViewData[name];
            });
            
        }

        protected  System.Security.Principal.IPrincipal Principal
        {
            get
            {
                return System.Threading.Thread.CurrentPrincipal;
            }
        }

        private Quacker _viewDataWrapper;
        protected IQuackFu Data
        {
            get { return _viewDataWrapper; }
        }

        private IQuackFu _utilsWrapper = new UtilsWrapper();

        protected IQuackFu ViewUtil
        {
            get { return _utilsWrapper; }
            set { _utilsWrapper = value; }
        }

        protected readonly object undefined = new object();

        #region IView Members

        

        public void Render(ViewContext viewContext, System.IO.TextWriter writer)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Encoding = writer.Encoding;
            xws.Indent = true;
            using (XmlWriter xw = XmlWriter.Create(writer, xws))
            {
                Render(viewContext, xw);
                xw.Flush();
            }
        }

        public void Render(ViewContext vc, XmlWriter writer)
        {
            DateTime dt = DateTime.Now;
            try
            {
                log = LogManager.GetLogger(this.GetType().Name + ".boml");
                Context = vc;
                _out = writer;
                DoRender();
            }
            catch (Exception ex)
            {
                log.Warn("Error: {0}", ex);
                throw;
            }
            finally
            {
                log.Debug("Rendering time: {0}", DateTime.Now - dt);
            }
        }

        #endregion

        private XmlWriter _out;

        protected XmlWriter Output
        {
            get { return _out; }
        }

        public delegate void Action();

        protected void doc(Action act)
        {
            _out.WriteStartDocument();
            act();
            _out.WriteEndDocument();
        }

        protected void processing_instruction(string name, string text)
        {
            _out.WriteProcessingInstruction(name, text);
        }

        protected void tag(string name, Action act)
        {
            _out.WriteStartElement(name);
            act();
            _out.WriteEndElement();
        }

        protected void tag(string prefix, string name, string ns, Action act)
        {
            _out.WriteStartElement(prefix, name, ns);
            act();
            _out.WriteEndElement();
        }

        protected void tag(string name, string ns, Action act)
        {
            _out.WriteStartElement(name, ns);
            act();
            _out.WriteEndElement();
        }

        protected void attr(string name, object val)
        {
            _out.WriteStartAttribute(name);
            _out.WriteValue(val);
            _out.WriteEndAttribute();
        }

        protected void attr(string name, string ns, object val)
        {
            _out.WriteStartAttribute(name, ns);
            _out.WriteValue(val);
            _out.WriteEndAttribute();
        }

        protected void attr(IDictionary attrs)
        {
            foreach (string k in attrs.Keys)
                attr(k, attrs[k]);
        }

        protected void tag(string name, object v)
        {
            _out.WriteStartElement(name);
            value(v);
            _out.WriteEndElement();
        }

        
        protected void value(object v)
        {
            if (v is string)
                val((string)v);
            else if (v is long)
                val((long)v);
            else if (v is int)
                val((int)v);
            else if (v is double)
                val((double)v);
            else if (v is float)
                val((float)v);
            else if (v is DateTime)
                val((DateTime)v);
            else if (v is bool)
                val((bool)v);
            else
                val(v.ToString());
        }

        protected void val(string val)
        {
            _out.WriteValue(val);
        }

        protected void val(bool val)
        {
            _out.WriteValue(val);
        }

        protected void val(DateTime val)
        {
            _out.WriteValue(val);
        }

        protected void val(long val)
        {
            _out.WriteValue(val);
        }

        protected void val(double val)
        {
            _out.WriteValue(val);
        }

        protected void val(int val)
        {
            _out.WriteValue(val);
        }

        protected void val(float val)
        {
            _out.WriteValue(val);
        }

        protected void val(decimal val)
        {
            _out.WriteValue(val);
        }

        protected void cdata(string txt)
        {
            _out.WriteCData(txt);
        }

        protected void raw(string txt)
        {
            _out.WriteRaw(txt);
        }





        

        protected void skip(params object[] prm)
        {

        }

        private string _viewPath;
        public string ViewPath
        {
            get { return _viewPath; }
            set { _viewPath = value; }
        }

        private JSONViewFactory _fact;
        internal JSONViewFactory Factory
        {
            get { return _fact; }
            set { _fact = value; }
        }

        internal delegate object QuackGetDelegate(string name, object[] parameters);
        internal delegate object QuackSetDelegate(string name, object[] parameters, object val);
        internal class Quacker : IQuackFu
        {
            private QuackGetDelegate _getter;
            private QuackGetDelegate _invoker;
            private QuackSetDelegate _setter;

            public Quacker(QuackGetDelegate dlg)
            {
                _getter = dlg;
            }
            public Quacker(QuackGetDelegate getter, QuackSetDelegate setter, QuackGetDelegate invoker)
            {
                _getter = getter;
                _setter = setter;
                _invoker = invoker;
            }

            public object QuackGet(string name, object[] parameters)
            {
                return _getter(name, parameters);
            }

            public object QuackInvoke(string name, params object[] args)
            {
                if (_invoker == null) throw new NotImplementedException(); 
                return _invoker(name, args);
            }

            public object QuackSet(string name, object[] parameters, object value)
            {
                if (_invoker == null) throw new NotImplementedException();
                return _setter(name, parameters, value);
            }
        }

        internal class UtilsWrapper : IQuackFu
        {
            private Hashtable _data = new Hashtable();
            #region IQuackFu Members

            public object QuackGet(string name, object[] parameters)
            {
                object v = _data[name];
                return v;
            }

            public object QuackInvoke(string name, params object[] args)
            {
                object v = _data[name];
                if (v == null) throw new Exception("Not found: " + name);
                if (v is ICallable)
                {
                    ICallable c = (ICallable)v;
                    return c.Call(args);
                }
                else if (v is Delegate)
                {
                    Delegate d = (Delegate)v;
                    return d.DynamicInvoke(args);
                }
                else throw new Exception("Cannot call " + name);
            }

            public object QuackSet(string name, object[] parameters, object value)
            {
                _data[name] = value;
                return null;
            }

            #endregion
        }

            

    }
}
