using System;
using System.Collections.Generic;
using System.Text;
using Rhino.DSL;
using System.Reflection;

namespace BosonMVC.Services.Boson
{
    class JSONViewDSLEngine : DslEngine
    {
        private Type _baseType = typeof(JSONViewBase);

        public JSONViewDSLEngine()
        {
            this.Storage = new FileStorageEx();
        }

        public string FileFormat
        {
            get { return ((FileSystemDslEngineStorage)Storage).FileNameFormat; }
            set { ((FileStorageEx)Storage).SetFileFormat(value); }
        }

        public Type BaseType
        {
            get { return _baseType; }
            set { _baseType = value; }
        }

        private string[] _namespaces = new string[] {
            "System.Data",
            "System", 
            "System.Web",
            "System.IO"
            };

        public string[] Namespaces
        {
            get { return _namespaces; }
            set { _namespaces = value; }
        }

        public bool AutoReferenceLoadedAssemblies { get; set; }

        protected override void CustomizeCompiler(Boo.Lang.Compiler.BooCompiler compiler, Boo.Lang.Compiler.CompilerPipeline pipeline, string[] urls)
        {
            compiler.Parameters.Ducky = true;
            
            List<Assembly> asms = new List<Assembly>();
            if (AutoReferenceLoadedAssemblies)
            {
                asms.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            }
            foreach (Assembly asm in asms)
            {
                try
                {
                    string t = asm.Location;
                }
                catch (Exception) { continue; }
                if (!compiler.Parameters.References.Contains(asm))
                    compiler.Parameters.References.Add(asm);
            }
            pipeline.Insert(1, new ImplicitBaseClassCompilerStep(
                _baseType, "PrepareView", _namespaces));
            pipeline.Insert(2, new AutoReferenceFilesCompilerStep());
        }
    }
}
