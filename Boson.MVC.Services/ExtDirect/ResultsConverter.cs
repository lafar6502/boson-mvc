using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BosonMVC.Services.DirectHandler
{
    internal class ResultConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JContainer o = value as JContainer;
            if (o != null)
            {
                writer.WriteRawValue(o.ToString(Formatting.None));
            }
            else
            {
                serializer.Serialize(writer, value);
                //writer.WriteValue(value);
            }
        }
    }
}
