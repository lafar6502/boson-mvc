using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace BosonMVC.Services.DirectHandler
{
    /// <summary>
    /// Direct action interface - marker interface
    /// for classes implementing direct actions
    /// </summary>
    public interface IDirectAction
    {
        
    }
    
    public interface IDirectActionDynamic
    {
        /// <summary>
        /// List of names of methods that can be invoked dynamically
        /// </summary>
        /// <returns></returns>
        string[] GetMethodNames();
        /// <summary>
        /// Method parameter information. Return null if you want any parameters to be passed 
        /// to Execute without any conversion or validation
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        ParameterInfo[] GetMethodParameters(string methodName);
        /// <summary>
        /// Execute specified method
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        object Execute(string methodName, object[] args);
    }
}
