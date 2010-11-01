using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BosonMVC.Services.DirectHandler
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DirectMethodAttribute : System.Attribute
    {
        public bool IsForm { get; set; }
        public bool PassRawParams { get; set; }
    }
}
