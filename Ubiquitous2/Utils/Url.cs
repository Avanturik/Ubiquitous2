using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace UB.Utils
{
    public class Url
    {
        public static string AppendParameter( string inputUrl, string parameter, string value )
        {
            if (String.IsNullOrWhiteSpace(parameter))
                return null;
            
            var uriBuilder = new UriBuilder(inputUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[parameter] = value;
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        public static string GetParameter (Uri uri, string parameterName )
        {
            var checkUri = uri;
            if (!checkUri.IsAbsoluteUri)
            {
                if (!Uri.TryCreate("http://localhost/" + uri.OriginalString, UriKind.Absolute, out checkUri))
                    return String.Empty;
            }
            var query = HttpUtility.ParseQueryString(checkUri.Query);
            return query[parameterName];
        }

        public static Uri RemoveParameters( Uri uri, string[] parameters )
        {
            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach( string parameter in parameters )
                query.Remove(parameter);

            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
            
        }
    }
}
