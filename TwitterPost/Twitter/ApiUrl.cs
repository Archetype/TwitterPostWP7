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
    public static class ApiUrl
    {
        /// <summary>
        /// Url to fetch the request token.
        /// </summary>
        public static string RequestTokenUrl 
        { 
            get 
            { 
                return string.Format("https://api.twitter.com/oauth/request_token"); 
            } 
        }

        /// <summary>
        /// Url to fetch the access token.
        /// </summary>
        public static string AccessTokenUrl 
        { 
            get 
            {
                return string.Format("https://api.twitter.com/oauth/access_token"); 
            } 
        }

        /// <summary>
        /// OAuth Signature Method.
        /// </summary>
        public static string OAuthSignatureMethod
        {
            get
            {
                return "HMAC-SHA1";
            }
        }

        /// <summary>
        /// Authentication Url.
        /// </summary>
        /// <param name="token">Authentication token from provided by twitter.</param>
        /// <returns></returns>
        public static string AuthenticationUrl(string token)
        { 
            return string.Format("http://api.twitter.com/oauth/authorize?oauth_token={0}", token); 
        }

        /// <summary>
        /// Callback Url.
        /// </summary>
        public static string CallbackUrl 
        { 
            get 
            {
                return "http://ap.org/apwp7";
                //return "sampleString/samples";
            } 
        }

        /// <summary>
        /// Base URL to update the twitter status.
        /// </summary>
        public static string StatusUpdateUrl        
        {
            get
            {
                return "http://api.twitter.com/1/statuses/update.xml";
            }
        }

        /// <summary>
        /// Url to Verify User Credentials.
        /// </summary>
        public static string VerifyCredentialsUrl 
        { 
            get 
            { 
                return "http://api.twitter.com/1/account/verify_credentials.xml"; 
            } 
        }
    }
}
