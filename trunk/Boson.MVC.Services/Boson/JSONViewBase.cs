using System;
using System.Collections;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using NLog;
using Boo.Lang;
using System.Data.SqlTypes;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace BosonMVC.Services.Boson
{
    public abstract class JSONViewBase : IView
    {
        protected abstract void PrepareView();
        protected ViewContext Context;
        protected Logger log = LogManager.GetCurrentClassLogger();

        public JSONViewBase()
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
            JsonTextWriter jsw = new JsonTextWriter(writer);
            jsw.Formatting = Formatting.Indented;
            Render(viewContext, jsw);
            jsw.Flush();
        }

        public void Render(ViewContext vc, JsonWriter writer)
        {
            DateTime dt = DateTime.Now;
            try
            {
                log = LogManager.GetLogger(this.GetType().Name + ".boson");
                Context = vc;
                _out = writer;
                PrepareView();
                if (_body != null)
                {
                    _body();
                }
            }
            catch (JsonViewException)
            {
                throw;
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

        private JsonWriter _out;

        protected JsonWriter Output
        {
            get { return _out; }
        }

        public delegate void Action();

        protected void obj(Action act)
        {
            _out.WriteStartObject();
            act();
            _out.WriteEndObject();
        }

        protected void obj(string name, Action act)
        {
            try
            {
                _out.WritePropertyName(name);
                _out.WriteStartObject();
                act();
                _out.WriteEndObject();
            }
            catch (Exception ex)
            {
                log.Error("Error writing property {0}: {1}", name, ex);
                throw new JsonViewException("Error writing property " + name, ex);
            }
        }

        protected void obj(IDictionary dic)
        {
            write_obj(dic);
        }

        protected static IDictionary apply(IDictionary left, IDictionary right)
        {
            Hashtable ret = new Hashtable();
            if (right == null) return left;
            foreach (string key in left.Keys)
            {
                ret[key] = left[key];
            }
            foreach (string key in right.Keys)
            {
                ret[key] = right[key];
            }
            return ret;
        }

        protected void arr(Action act)
        {
            _out.WriteStartArray();
            act();
            _out.WriteEndArray();
        }

        protected void arr(ICollection c)
        {
            write_obj(c);
        }

        protected void arr(string name, Action act)
        {
            try
            {
                _out.WritePropertyName(name);
                _out.WriteStartArray();
                act();
                _out.WriteEndArray();
            }
            catch (Exception ex)
            {
                log.Error("Error writing array {0}: {1}", name, ex);
            }
        }

        protected void val(object v)
        {
            write_obj(v);
        }

        protected void raw(string js)
        {
            _out.WriteRaw(js);
        }

        protected void raw(string name, string js)
        {
            try
            {
                _out.WritePropertyName(name);
                _out.WriteRawValue(js);
            }
            catch (Exception ex)
            {
                log.Error("Error writing raw property {0}={1}: {2}", name, js, ex);
                throw new JsonViewException("Error writing raw " + name, ex); ;
            }
        }
        

        protected void prop(string name, object value)
        {
            try
            {
                _out.WritePropertyName(name);
                write_obj(value);
            }
            catch (Exception ex)
            {
                log.Error("Error writing property {0}={1}: {2}", name, value, ex);
                throw new JsonViewException(string.Format("Error writing prop {0}={1}", name, value), ex);
            }
        }

        protected void prop(string name, Action act)
        {
            try
            {
                _out.WritePropertyName(name);
                if (act != null)
                    act();
                else
                    _out.WriteNull();
            }
            catch (Exception ex)
            {
                log.Error("Error writing property {0}: {1}", name, ex);
                throw new JsonViewException("Error writing property " + name, ex);
            }
        }

        protected void prop(IDictionary dic)
        {
            foreach (string key in dic.Keys)
            {
                prop(key, dic[key]);
            }
        }

        protected void newobj(string name, Action act)
        {
            try
            {
                _out.WriteStartConstructor(name);
                JsonWriter o = _out;
                StringWriter sw = new StringWriter();
                _out = new JsonTextWriter(sw);
                act();
                _out.Flush();
                _out = o;
                _out.WriteRaw(sw.ToString());
                _out.WriteEndConstructor();
            }
            catch (Exception ex)
            {
                log.Error("Error writing new object {0}: {1}", name, ex);
                throw new JsonViewException("Error writing new object " + name, ex);
            }
        }

        protected void callfn(string name, Action act)
        {
            try
            {
                //_out.WriteStartConstructor(name);
                JsonWriter o = _out;
                StringWriter sw = new StringWriter();
                _out = new JsonTextWriter(sw);
                act();
                _out.Flush();
                _out = o;
                _out.WriteRaw(name + "(" + sw.ToString() + ")");
                //_out.WriteRaw(sw.ToString());
                //_out.WriteEndConstructor();
            }
            catch (Exception ex)
            {
                log.Error("Error writing function call {0}: {1}", name, ex);
                throw new JsonViewException("Error writing function call " + name, ex);
            }
        }

        protected void skip(params object[] prm)
        {

        }

        protected string escapeXml(string xml)
        {
            return xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private void write_obj(object ob)
        {
            if (ob is IDictionary)
            {
                IDictionary d = (IDictionary)ob;
                _out.WriteStartObject();
                foreach (string key in d.Keys)
                {
                    prop(key, d[key]);
                }
                _out.WriteEndObject();
            }
            else if (ob is ICollection)
            {
                ICollection c = ob as ICollection;
                _out.WriteStartArray();
                foreach (object v in c)
                    write_obj(v);
                _out.WriteEndArray();
            }
            else if (ob is INullable)
            {
                INullable n = ob as INullable;
                if (n.IsNull)
                    _out.WriteNull();
                else
                {
                    System.Reflection.PropertyInfo pi = ob.GetType().GetProperty("Value");
                    val(pi.GetValue(ob, null));
                    //_out.WriteValue(pi.GetValue(ob, null));
                }
            }
            else if (ob == null)
            {
                _out.WriteNull();
            }
            else if (ob == undefined)
            {
                _out.WriteUndefined();
            }
            else if (ob is ValueType || ob is string)
            {
                if (ob is DateTime)
                {
                    ob = ((DateTime)ob).ToString("yyyy-MM-dd HH:mm:ss");
                }
                _out.WriteValue(ob);
                //_out.WriteValue(ob);
            }
            else
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(_out, ob);
            }
        }

        protected void include(string vname)
        {
            if (!vname.StartsWith("~"))
            {
                string basePath = Path.GetDirectoryName(this.ViewPath);
                vname = Path.Combine(basePath, vname);
            }
            JSONViewBase vb = (JSONViewBase) Factory.CreateView(vname);
            vb.Context = Context;
            vb._out = _out;
            vb.ViewUtil = this.ViewUtil;
            vb.PrepareView();
            foreach (string tn in vb._templates.Keys)
            {
                log.Debug("Imported template {0} from {1}", tn, vname);
                this._templates[tn] = vb._templates[tn];
            }
        }


        [DuckTyped]
        protected object T { get; set; }
        protected IQuackFu Tparam { get; set; }


        public delegate object TemplateDelegate(object value, IQuackFu parameters);

        protected Hashtable _templates = new Hashtable();

        protected void define_template(string name, TemplateDelegate act)
        {
            _templates[name] = act;
        }

        /// <summary>
        /// Call specified template
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="parameters"></param>
        protected void call_template(string name, object value, IDictionary parameters)
        {
            TemplateDelegate td = (TemplateDelegate)_templates[name];
            if (td == null) throw new Exception("Template not found: " + name);

            IQuackFu q = new QuackDictWrapper(parameters);
            object ret = td(value, q);
            if (ret != null)
            {
                write_obj(ret);
            }
        }

        private Action _body;

        /// <summary>
        /// Define view 'body', that is its output generating method.
        /// If you don't define the body nothing bad happens but includes might work in strange way
        /// </summary>
        /// <param name="act"></param>
        protected void body(Action act)
        {
            _body = act;
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

        /// <summary>
        /// Calls a template in specified view
        /// </summary>
        /// <param name="name"></param>
        /// <param name="inputValue"></param>
        /// <param name="parameters"></param>
        public void CallTemplate(string name, object inputValue, IQuackFu parameters)
        {
            
        }

    }
}
