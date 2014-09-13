using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UB.Model;

namespace UB.Utils
{
    public class Json
    {
        public static T DeserializeUrl<T>(string url) where T:class
        {
            using (WebClientBase wc = new WebClientBase())
            using (Stream stream = wc.DownloadToStream(url))
            {
                if (stream == null)
                    return null;

                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonReader reader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    try
                    {
                        return serializer.Deserialize<T>(reader);
                    }
                    catch
                    {
                        Log.WriteError("Deserializing of {0} failed", typeof(T).ToString());
                        return null;
                    }
                }

            }

        }
    }
}
