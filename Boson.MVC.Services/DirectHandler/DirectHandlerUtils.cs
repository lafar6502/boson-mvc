using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using NLog;
using Castle.Windsor;
using Castle.MicroKernel;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace BosonMVC.Services.DirectHandler
{
    public class DirectHandlerUtils
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void OutputDirectAPI(HttpContext ctx, TextWriter tw, IKernel serviceLocator)
        {
            JsonWriter jw = new JsonTextWriter(tw);
            ///produce API here
            ///
            jw.WriteStartObject();
            jw.WritePropertyName("type"); jw.WriteValue("remoting");
            jw.WritePropertyName("url"); jw.WriteValue(ctx.Request.Path);

            List<string> names = new List<string>();
            IHandler[] hs = serviceLocator.GetHandlers(typeof(IDirectAction));
            foreach (IHandler ih in hs)
            {
                string name = ih.ComponentModel.Name;
                if (name == null || name.Length == 0) throw new Exception("Found IDirectAction with no name - fix your component configuration");
                names.Add(name);
            }

            jw.WritePropertyName("actions");
            jw.WriteStartObject();
            foreach (string name in names)
            {
                IDirectAction act = serviceLocator.Resolve<IDirectAction>(name);
                if (act == null) throw new Exception("Failed to resolve IDirectAction: " + name);
                Type tp = act.GetType();
                jw.WritePropertyName(name);
                jw.WriteStartArray();
                foreach (MethodInfo mi in tp.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod))
                {
                    DirectMethodAttribute dm = (DirectMethodAttribute)Attribute.GetCustomAttribute(mi, typeof(DirectMethodAttribute));
                    if (dm != null)
                    {
                        jw.WriteStartObject();
                        jw.WritePropertyName("name"); jw.WriteValue(mi.Name);
                        jw.WritePropertyName("len"); jw.WriteValue(mi.GetParameters().Length);
                        if (dm.IsForm)
                        {
                            jw.WritePropertyName("formHandler");
                            jw.WriteValue(true);
                        }
                        jw.WriteEndObject();
                    }
                }
                jw.WriteEndArray();
            }
            jw.WriteEndObject();
            jw.WriteEndObject();
            jw.Flush();
        }

        public static DirectResponse ProcessRequest(DirectRequest drq, IKernel serviceLocator)
        {
            log.Info("Processing request {0}: {1}:{2}. Args: {3}", drq.TransactionId, drq.Action, drq.Method, drq.Data.Length);
            DirectResponse r = new DirectResponse(drq);
            try
            {
                IDirectAction ida = serviceLocator.Resolve<IDirectAction>(drq.Action);
                if (ida == null) throw new Exception("Action not found: " + drq.Action);
                object ret = DefaultExecuteActionMethod(drq, ida);
                log.Info("Finished processing request {0}: {1}:{2}", drq.TransactionId, drq.Action, drq.Method);
                if (ret is DirectResponse)
                {
                    r = (DirectResponse)ret;
                }
                else
                {
                    r.Result = ret;
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error processing request {0}: {1}:{2}: {3}", drq.TransactionId, drq.Action, drq.Method, ex);
                if (ex is TargetInvocationException)
                {
                    ex = ((TargetInvocationException)ex).InnerException;
                }
                r.ExceptionMessage = ex.Message;
                r.Type = DirectResponse.ResponseExceptionType;
            }
            return r;
        }

        /// <summary>
        /// Invoke the method on action object. Handles also dynamic invocation through IDirectActionDynamic
        /// </summary>
        /// <param name="drq"></param>
        /// <param name="ida"></param>
        /// <returns></returns>
        private static object DefaultExecuteActionMethod(DirectRequest drq, IDirectAction ida)
        {
            JsonSerializer ser = JsonSerializer.Create(GetSerializerSettings());
            ParameterInfo[] prm = null;
            IDirectActionDynamic idad = ida as IDirectActionDynamic;
            bool dontConvertParams = false;
            MethodInfo mi = ida.GetType().GetMethod(drq.Method);

            if (mi == null)
            {
                if (idad != null)
                {
                    string[] mths = idad.GetMethodNames();
                    if (mths.Contains(drq.Method))
                    {
                        prm = idad.GetMethodParameters(drq.Method);
                        dontConvertParams = prm == null;
                    }
                }
                throw new Exception("Method not found: " + drq.Method);
            }
            else
            {
                prm = mi.GetParameters();
            }


            List<object> paramVals = new List<object>();
            if (dontConvertParams)
            {
                foreach (object d in drq.Data)
                    paramVals.Add(d);
            }
            else
            {
                if (prm.Length != drq.Data.Length)
                {
                    /*if (prm.Length > drq.Data.Length)
                        throw new Exception("Incorrect number of parameters to method " + drq.Method + ". #Arguments expected: " + prm.Length);
                    else
                        log.Warn("Method {0}.{1} expects {2} arguments and {3} were supplied. ", drq.Action, drq.Method, prm.Length, drq.Data.Length);
                */
                }
                for (int i = 0; i < prm.Length; i++)
                {
                    if (prm[i].ParameterType == typeof(DirectRequest))
                    {
                        paramVals.Add(drq);
                    }
                    else if (drq.Data[i] is JToken)
                    {
                        JToken jt = (JToken)drq.Data[i];
                        if (typeof(JToken).IsAssignableFrom(prm[i].ParameterType))
                            paramVals.Add(jt);
                        else
                            paramVals.Add(ser.Deserialize(jt.CreateReader(), prm[i].ParameterType));
                    }
                    else if (prm[i].ParameterType.IsArray)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (drq.Data[i] == null)
                            paramVals.Add(null);
                        else
                        {
                            TypeConverter conv = TypeDescriptor.GetConverter(prm[i].ParameterType);
                            if (conv.CanConvertFrom(drq.Data[i].GetType()))
                                paramVals.Add(conv.ConvertFrom(drq.Data[i]));
                            else
                            {
                                paramVals.Add(Convert.ChangeType(drq.Data[i], prm[i].ParameterType));

                                //paramVals.Add(drq.Data[i]);
                            }
                        }
                    }
                }
            }
            if (mi != null)
                return mi.Invoke(ida, paramVals.ToArray());
            else
                return idad.Execute(drq.Method, paramVals.ToArray());
        }

        public static JsonSerializerSettings GetSerializerSettings()
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();
            return sett;
        }

        public static void ProcessRequest(HttpContext context, IKernel serviceLocator, string apiNamespace)
        {

            string data = string.Empty;
            string type = "text/javascript";

            try
            {
                if (context.Request.TotalBytes == 0 && string.IsNullOrEmpty(context.Request["extAction"]))
                {
                    StringWriter sw = new StringWriter();
                    OutputDirectAPI(context, sw, serviceLocator);
                    string api = apiNamespace;
                    if (api == null || api.Length == 0)
                    {
                        log.Warn("Ext.Direct API - javascript namespace not specified. Specify 'NGExt.Services.DirectHandler.ApplicationNamespace' key in appsettings section in config file");
                        api = "Application.app.DIRECT_API";
                    }
                    data = string.Format("{0} = {1}", api, sw.ToString());
                }
                else
                {
                    List<DirectResponse> responses = new List<DirectResponse>();
                    List<DirectRequest> requests = new List<DirectRequest>();
                    if (!String.IsNullOrEmpty(context.Request[DirectRequest.RequestFormAction]))
                    {
                        DirectRequest request = new DirectRequest();
                        request.Action = context.Request[DirectRequest.RequestFormAction] ?? string.Empty;
                        request.Method = context.Request[DirectRequest.RequestFormMethod] ?? string.Empty;
                        request.Type = context.Request[DirectRequest.RequestFormType] ?? string.Empty;
                        request.IsUpload = Convert.ToBoolean(context.Request[DirectRequest.RequestFormUpload]);
                        request.TransactionId = Convert.ToInt32(context.Request[DirectRequest.RequestFormTransactionId]);
                        request.Data = new object[] { context.Request };
                        requests.Add(request);
                        //responses.Add(ProcessRequest(request));
                    }
                    else
                    {
                        UTF8Encoding encoding = new UTF8Encoding();
                        string json = encoding.GetString(context.Request.BinaryRead(context.Request.TotalBytes));
                        log.Debug("Processing JSON: {0}", json);
                        if (json.StartsWith("["))
                        {
                            List<DirectRequest> rl = JsonConvert.DeserializeObject<List<DirectRequest>>(json, DirectHandlerUtils.GetSerializerSettings());
                            requests.AddRange(rl);
                        }
                        else
                        {
                            DirectRequest drq = JsonConvert.DeserializeObject<DirectRequest>(json, DirectHandlerUtils.GetSerializerSettings());
                            requests.Add(drq);
                        }
                    }
                    //now calling it
                    foreach (DirectRequest drq in requests)
                    {
                        responses.Add(ProcessRequest(drq, serviceLocator));
                    }
                    //now return the results

                    if (responses.Count > 1)
                    {
                        data = JsonConvert.SerializeObject(responses, Formatting.Indented, DirectHandlerUtils.GetSerializerSettings());
                    }
                    else
                    {
                        DirectResponse r = responses[0];
                        if (r.IsUpload)
                        {
                            type = "text/html";
                            data = String.Format("<html><body><textarea>{0}</textarea></body></html>", JsonConvert.SerializeObject(r, Formatting.Indented, DirectHandlerUtils.GetSerializerSettings()).Replace("&quot;", "\\&quot;"));
                        }
                        else
                        {
                            data = JsonConvert.SerializeObject(r, Formatting.Indented, DirectHandlerUtils.GetSerializerSettings());
                        }
                    }
                }
                log.Debug("Response\nContent-Type: {0}\n{1}", type, data);
                context.Response.ContentType = type;
                context.Response.Write(data);
            }
            catch (Exception ex)
            {
                log.Error("Error handling direct rpc request: {0}", ex);
                throw;
            }
        }
    }

}
