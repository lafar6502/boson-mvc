using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
using NLog;
using Rhino.DSL;
using System.IO;
using System.Web;

namespace BosonMVC.Services.Boson
{
    public class JSONViewFactory : System.Web.Mvc.VirtualPathProviderViewEngine, IViewEngine
    {
        protected Logger log = LogManager.GetCurrentClassLogger();
        private DslFactory _engine = new DslFactory();
        private JSONViewDSLEngine _bosonEngine;
        private JSONViewDSLEngine _bomlEngine;
        
        public JSONViewFactory()
        {
            base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.boson", "~/Views/{1}/{0}.boml" };
            base.PartialViewLocationFormats = base.ViewLocationFormats;
            log.Info("JSONViewFactory created. View locations: {0}", base.ViewLocationFormats);
            _bosonEngine = new JSONViewDSLEngine();
            _bomlEngine = new JSONViewDSLEngine();
            _bosonEngine.Namespaces = new string[] {
                "System", "System.IO", "System.Text", "System.Data"
            };
            _bomlEngine.Namespaces = _bosonEngine.Namespaces;
            _bosonEngine.BaseType = typeof(JSONViewBase);
            _bomlEngine.FileFormat = "*.boson";
            _bomlEngine.BaseType = typeof(BOMLViewBase);
            _bomlEngine.FileFormat = "*.boml";
            _engine.Register<JSONViewBase>(_bosonEngine); 
            _engine.Register<BOMLViewBase>(_bomlEngine);
            _engine.Compilation += new EventHandler(_engine_Compilation);
            _engine.Recompilation += new EventHandler(_engine_Recompilation);
        }

        void _engine_Recompilation(object sender, EventArgs e)
        {
            log.Warn("Recompilation in {0}", _engine.BaseDirectory);

        }

        void _engine_Compilation(object sender, EventArgs e)
        {
            log.Warn("Compilation in {0}", _engine.BaseDirectory);
        }

        public string BaseDirectory
        {
            get { return _engine.BaseDirectory; }
            set { _engine.BaseDirectory = value; }
        }

        public IServiceResolver ServiceLocator { get; set; }

        public string[] Namespaces
        {
            get
            {
                return _bosonEngine.Namespaces;
            }
            set
            {
                _bosonEngine.Namespaces = value;
                _bomlEngine.Namespaces = value;
            }
        }

        public bool AutoReferenceLoadedAssemblies
        {
            get { return _bosonEngine.AutoReferenceLoadedAssemblies; }
            set
            {
                _bosonEngine.AutoReferenceLoadedAssemblies = value;
                _bomlEngine.AutoReferenceLoadedAssemblies = value;
            }
        }

        private string _fileExt = ".boson";
        
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            log.Info("CreatePartialView: {0}", partialPath);
            return CreateView(controllerContext, partialPath, null);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return CreateView(viewPath) ;
        }

        public IView CreateView(string viewPath)
        {
            log.Info("CreateView: path={0}", viewPath);
            if (viewPath.StartsWith("~/")) viewPath = viewPath.Substring(2);
            if (viewPath.EndsWith(".boml"))
            {
                BOMLViewBase b = _engine.Create<BOMLViewBase>(viewPath, null);
                b.Factory = this;
                b.ViewPath = viewPath;
                return b;
            }
            else if (viewPath.EndsWith(".boson"))
            {
                JSONViewBase b = _engine.Create<JSONViewBase>(viewPath, null);
                b.Factory = this;
                b.ViewPath = viewPath;
                return b;
            }
            else throw new Exception("Unrecognized view: " + viewPath);
        }

        public void RenderView(string viewName, ViewContext vc, TextWriter output)
        {
            IView iv = CreateView(viewName);
            iv.Render(vc, output);
        }

    }
}
