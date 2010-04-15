using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace BosonMVC.Services
{
    /// <summary>
    /// Callback result - allows to specify callback to be executed after
    /// the view is executed
    /// </summary>
    public class CallbackResult : ActionResult
    {
        private Action _act;
        private ActionResult _wrapped;

        public CallbackResult(ActionResult wrap, Action act)
            : base()
        {
            _wrapped = wrap;
            _act = act;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            try
            {
                _wrapped.ExecuteResult(context);
            }
            finally
            {
                if (_act != null) _act();
            }
        }
    }
}
