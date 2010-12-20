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
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Twitter
{
    public static class Helpers
    {
        internal static string NormalizeAllParams(Dictionary<string, string> oAuthParams, Dictionary<string, string> queryParams)
        {
            Dictionary<string, string> allParams = new Dictionary<string, string>(oAuthParams);

            if (queryParams != null)
            {
                // For some reason we have to escape the query params twice 

                foreach (string k in queryParams.Keys)
                {
                    if (!allParams.ContainsKey(k))
                    {
                        allParams.Add(UriEscape(k), UriEscape(queryParams[k]));
                    }
                }
            }

            if ((allParams == null) || (allParams.Keys.Count == 0))
            {
                throw new Exception("Could not construct query string from empty or null param dictionary");
            }

            List<string> keys = new List<string>(allParams.Keys);

            StringBuilder sb = new StringBuilder();

            keys.Sort(delegate(string a, string b)
            {
                int difference = a.CompareTo(b);

                if (difference == 0)
                {
                    return allParams[a].CompareTo(allParams[b]);
                }
                else
                {
                    return difference;
                }
            });

            int i = 0;
            if (keys.Count > 1)
            {
                for (; i < keys.Count - 1; i++)
                {
                    sb.Append(UriEscape(keys[i]));
                    sb.Append("%3D");
                    sb.Append(UriEscape(allParams[keys[i]]));
                    sb.Append("%26");
                }
            }

            sb.Append(UriEscape(keys[i]));
            sb.Append("%3D");
            sb.Append(UriEscape(allParams[keys[i]]));

            return sb.ToString();
        }

        internal enum HttpMethod
        {
            Post,
            Get
        }

        internal static string GetSignatureBase(HttpMethod method, string baseUrl, string normalizedQueryParams)
        {
            StringBuilder sb = new StringBuilder();

            string methodStr = method == HttpMethod.Get ? "GET" : "POST";
            sb.Append(methodStr);
            sb.Append("&");
            sb.Append(Helpers.UriEscape(baseUrl));
            sb.Append("&");
            sb.Append(normalizedQueryParams);

            return sb.ToString();
        }

        /// <summary>
        /// RFC 3986 compliant Uri escape a string.
        /// Thanks to http://blog.nerdbank.net/2009/05/uriescapedatapath-and.html
        /// </summary>
        /// <param name="uri">Uri string to escape.</param>
        /// <returns>The escaped string.</returns>
        internal static string UriEscape(string uri)
        {
            string[] entities = new[] { "!", "*", "'", "(", ")" };
            string[] replace = new[] { "%21", "%2A", "%27", "%28", "%29" };

            StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(uri));

            // Upgrade the escaping to RFC 3986, if necessary.
            for (int i = 0; i < entities.Length; i++)
            {
                escaped.Replace(entities[i], replace[i]);
            }

            return escaped.ToString();
        }

        internal static string GenerateNonce()
        {
            SHA1Managed sha1 = new System.Security.Cryptography.SHA1Managed();

            string nonce = (DateTime.Now.Ticks % 123456).ToString();

            Byte[] hash = sha1.ComputeHash(new UTF8Encoding().GetBytes(nonce));

            StringBuilder sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        internal static string TimeStamp()
        {
            DateTime unixStart = new DateTime(1970, 1, 1);
            DateTime current = DateTime.UtcNow;

            TimeSpan diff = current - unixStart;

            return ((long)diff.TotalSeconds).ToString();
        }

        internal static string GetSignature(string baseSignature, string consumerSecret, string requestTokenSecret)
        {
            StringBuilder sb = new StringBuilder(consumerSecret);

            sb.Append("&");

            if (requestTokenSecret != null)
            {
                sb.Append(requestTokenSecret);
            }

            byte[] key = Encoding.UTF8.GetBytes(sb.ToString());

            //Use the key to encrypt the signature
            HMACSHA1 sigMethod = new HMACSHA1(key);

            byte[] baseBytes = Encoding.UTF8.GetBytes(baseSignature);

            //Base64 the signature
            return Convert.ToBase64String(sigMethod.ComputeHash(baseBytes));
        }


        /// <summary>
        /// Get a Url's parameters leaving them url encoded
        /// </summary>
        /// <param name="urlString">The query string</param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseQueryString(string urlString)
        {
            string queryString = new Uri(urlString).Query;
            Dictionary<string, string> kVParms = new Dictionary<string, string>();

            queryString.Replace("?", "");

            string[] keyValPairing = queryString.Split('&', '=');

            if ((keyValPairing.Length == 0) || (keyValPairing.Length % 2 != 0))
            {
                throw new Exception("Invalid input Url");
            }

            keyValPairing[0] = keyValPairing[0].TrimStart('?');

            for (int i = 0; i < keyValPairing.Length - 1; i++)
            {
                kVParms.Add(keyValPairing[i], keyValPairing[i + 1]);
            }

            return kVParms;
        }
    }
}
