using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace BosonMVC.Services.DirectHandler
{
    [JsonObject]
    public class DirectRequest
    {

        public const string RequestFormTransactionId = "extTID";
        public const string RequestFormAction = "extAction";
        public const string RequestFormMethod = "extMethod";
        public const string RequestFormUpload = "extUpload";
        public const string RequestFormType = "extType";

        public string Action
        {
            get;
            set;
        }

        public string Method
        {
            get;
            set;
        }

        [JsonConverter(typeof(RequestDataConverter))]
        public object[] Data
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public bool IsForm
        {
            get;
            set;
        }

        public bool IsUpload
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "tid")]
        public int TransactionId
        {
            get;
            set;
        }

    }
}
