using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BosonMVC.Services.DirectHandler
{
    class RequestDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsArray;
        }

        private List<object> Unpack(JArray jar)
        {
            List<object> lst = new List<object>();
            JToken tok = jar.First;
            while (tok != null)
            {
                if (tok is JObject)
                    lst.Add(tok);
                else if (tok is JArray)
                    lst.Add(Unpack((JArray) tok));
                else if (tok is JValue)
                    lst.Add(((JValue)tok).Value);
                else throw new Exception("Unhandled token type: " + tok.GetType());
                tok = tok.Next;
            }
            return lst;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray jar = JArray.Load(reader);
            List<object> lst = Unpack(jar);
            return lst.ToArray();
        }

        

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
