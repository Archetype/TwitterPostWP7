using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Linq;

namespace AP.WP7.Twitter
{
    public class Bitly
    {
        public static void Shorten(string url, Action<string> cb)
        {
            string bitlyApi = "http://api.bit.ly/v3/shorten?login=apnews1&apiKey=R_b048368185560f8af857be18e993a089&format=xml&longUrl=" + HttpUtility.UrlEncode(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(bitlyApi);

            request.Method = "GET";
            request.AllowReadStreamBuffering = true;

            SendRequest(request, cb);
        }

        internal static void SendRequest(HttpWebRequest requestContainer, Action<string> cb)
        {
            // Note that current design assumes a successfull post will be followed by a getAndRead
            AsyncCallback getAndRead = new AsyncCallback(delegate(IAsyncResult responseResult)
            {
                HttpWebRequest responseContainer = (HttpWebRequest)responseResult.AsyncState;

                try
                {
                    string url = null;
                    HttpWebResponse response = (HttpWebResponse)responseContainer.EndGetResponse(responseResult);

                    Stream s = response.GetResponseStream();

                    XDocument xmlDoc = XDocument.Load(s);

                    // Release the HttpWebResponse
                    response.Close();

                    if (xmlDoc.Element("response") != null)
                    {
                        if (xmlDoc.Element("response").Element("data") != null)
                        {
                            if (xmlDoc.Element("response").Element("data").Element("url") != null)
                            {
                                url = xmlDoc.Element("response").Element("data").Element("url").Value;
                            }
                        }
                    }
                    
                    cb(url);
                }
                catch (WebException we)
                {
                    string error = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();

                    cb(null);
                }
                catch (Exception e)
                {
                    cb(null);
                }
            });

            try
            {
                requestContainer.BeginGetResponse(getAndRead, requestContainer);
            }
            catch (Exception e)
            {
                cb(null);
            }
        }
    }
}
