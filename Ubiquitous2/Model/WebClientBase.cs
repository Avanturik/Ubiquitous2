using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Mime;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public enum ContentType
    {
        UrlEncoded,
        UrlEncodedUTF8,
        Multipart
    }
    public class WebClientBase : WebClient
    {
            private object downloadLock = new object();
            private readonly CookieContainer m_container = new CookieContainer();
            private const string userAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:14.0) Gecko/20100101 Firefox/14.0.1";
            private Dictionary<ContentType, string> contentTypes = new Dictionary<ContentType,string>() {
                 {ContentType.UrlEncodedUTF8, "application/x-www-form-urlencoded; charset=UTF-8"},
                 {ContentType.UrlEncoded, "application/x-www-form-urlencoded"},
                 {ContentType.Multipart, "multipart/form-data"},

            };
            public WebClientBase()
            {

                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(
                        delegate
                        { return true; }
                    );
                ServicePointManager.DefaultConnectionLimit = 10;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.UseNagleAlgorithm = false;
                KeepAlive = true;
                ErrorHandler = (error) => {
                    Log.WriteError(error);
                };
                StartPos = -1;
                EndPos = -1;
                
                Proxy = null;
                SuccessHandler = () => { };
            }
            public Action<string> ErrorHandler { get; set; }
            public Action SuccessHandler { get; set; }
            public bool KeepAlive { get; set; }

            public long StartPos { get; set; }
            public long EndPos { get; set; }
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);

                HttpWebRequest webRequest = request as HttpWebRequest;
                if (webRequest != null)
                {
                    if (KeepAlive)
                    {
                        webRequest.ProtocolVersion = HttpVersion.Version11;
                        webRequest.KeepAlive = true;
                        var sp = webRequest.ServicePoint;
                        var prop = sp.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                        prop.SetValue(sp, (byte)0, null);
                    }

                    if( StartPos != -1 && EndPos != -1 )
                    {
                        webRequest.AddRange(StartPos, EndPos);
                    }

                    webRequest.CookieContainer = m_container;
                    webRequest.UserAgent = userAgent;
                    webRequest.Proxy = null;
                    webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                return request;
            }

            public String Download(String url)
            {
                try
                {
                    lock (downloadLock)
                    {
                        SuccessHandler();
                        return DownloadString(new Uri(url));
                    }
                }
                catch
                {
                    ErrorHandler(String.Format("Error downloading {0}", url));
                }
                return String.Empty;
            }
            public Stream DownloadToStream(String url, bool cache = false)
            {
                try
                {
                    lock (downloadLock)
                    {
                        var request = GetWebRequest(new Uri(url));
                        if( cache )
                        {
                            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                        }
                        var response = GetWebResponse(request);
                        
                        SuccessHandler();
                        return response.GetResponseStream();
                    }
                }
                catch
                {
                    ErrorHandler(String.Format("Error downloading {0} to stream", url));
                }
                return null;
            }
            
            public String Upload(string url, string args)
            {
                string result = null;
                try
                {
                    lock (downloadLock)
                    {
                        result = UploadString(url, args);
                        SuccessHandler();
                    }
                }
                catch
                {
                    ErrorHandler(String.Format("Error uploading to {0}", url));
                }
                return result;
            }

            public Stream PutStream(string url, Stream stream)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "PUT";
                    if (stream != null)
                    {
                        request.ContentLength = stream.Length;
                        Stream dataStream = request.GetRequestStream();
                        stream.CopyTo(dataStream);
                        stream.Flush();
                        dataStream.Flush();
                        dataStream.Close();
                    }

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    return response.GetResponseStream();
                }
                catch
                {
                    return null;
                }
            }

            public void SetCookie(string name, string value, string domain)
            {
                if (name == null || value == null)
                    return;
                m_container.Capacity += 1;
                m_container.Add(new Cookie(name, value, "/", domain));
            }


            public ContentType ContentType
            {
                set { this.Headers[HttpRequestHeader.ContentType] = contentTypes[value]; }
            }

            public string CookieValue(string name, string url)
            {
                var coll = m_container.GetCookies(new Uri(url));
                if (coll == null || coll[name] == null)
                    return String.Empty;

                return coll[name].Value;
            }

            public CookieContainer Cookies
            {
                get { return m_container; }
                set
                {
                    Hashtable table = (Hashtable)value.GetType().InvokeMember("m_domainTable",
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.GetField |
                                                                 BindingFlags.Instance,
                                                                 null,
                                                                 value,
                                                                 new object[] { });
                    foreach (var key in table.Keys)
                    {
                        var url = String.Format("http://{0}/", key.ToString().TrimStart('.'));

                        foreach (Cookie cookie in value.GetCookies(new Uri(url)))
                        {
                            m_container.Add(cookie);
                        }
                    }


                }
            }        
            public long GetContentLength( string url )
            {
                lock(downloadLock)
                {
                    try
                    {
                        WebRequest request = GetWebRequest(new Uri(url));
                        //request.Method = "HEAD";
                        StartPos = -128;
                        EndPos = -128;
                        WebResponse result = request.GetResponse();
                        StartPos = -1;
                        EndPos = -1;
                        if (result != null)
                            return result.ContentLength;
                    }
                    catch
                    {
                        StartPos = -1;
                        EndPos = -1;
                    }
                    return -1;
                }
            }

            public Stream DownloadPartial( string url, long startPos, long endPos )
            {
                lock (downloadLock)
                {
                    try
                    {
                        StartPos = startPos;
                        EndPos = endPos;
                        WebRequest request = GetWebRequest(new Uri(url));
                        WebResponse result = request.GetResponse();
                        StartPos = -1;
                        EndPos = -1;
                        return result.GetResponseStream();
                    }
                    catch
                    {
                        StartPos = -1;
                        EndPos = -1;
                    }
                    return null;
                }
            }

    }
}