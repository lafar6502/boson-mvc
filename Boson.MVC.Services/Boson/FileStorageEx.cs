using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.DSL;

namespace BosonMVC.Services.Boson
{
    class FileStorageEx : FileSystemDslEngineStorage
    {
        private string _fileFormat = "*.boson";
        public override string FileNameFormat { get { return _fileFormat; } }
        public void SetFileFormat(string fmt) { _fileFormat = fmt; }
    }
}
