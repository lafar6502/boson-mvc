using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BosonMVC.Services.Boson
{
    [Serializable]
    public class JsonViewException : Exception
    {
        public JsonViewException(string msg, Exception ex)
            : base(msg, ex)
        {
        }
    }
}
