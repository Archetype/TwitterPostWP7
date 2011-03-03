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
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Browser;

namespace Twitter
{
    public class Account
    {
        const string kTwitterCredential = "Stored-Twitter-Credential";

        #region Stored

        public string OAuthRequestToken
        {
            get
            {
                return Stored.OAuthRequestToken;
            }
            set
            {
                Stored.OAuthRequestToken = value;
            }
        }

        public string OAuthRequestTokenSecret
        {
            get
            {
                return Stored.OAuthRequestTokenSecret;
            }
            set
            {
                Stored.OAuthRequestTokenSecret = value;
            }
        }

        public string OAuthAccessToken
        {
            get
            {
                return Stored.OAuthAccessToken;
            }
            set
            {
                Stored.OAuthAccessToken = value;
            }
        }

        public string OAuthAccessTokenSecret
        {
            get
            {
                return Stored.OAuthAccessTokenSecret;
            }
            set
            {
                Stored.OAuthAccessTokenSecret = value;
            }
        }

        public string ScreenName
        {
            get
            {
                return Stored.ScreenName;
            }
            set
            {
                Stored.ScreenName = value;
            }
        }

        #endregion

        #region Configuration Settings

        private static string m_CallbackUrl;
        public static string CallbackUrl
        {
            get
            {
                return m_CallbackUrl;
            }
            private set
            {
                if (m_CallbackUrl == value)
                {
                    return;
                }
                m_CallbackUrl = value;
            }
        }

        private static string m_ApiKey;
        public static string ApiKey
        {
            get
            {
                return m_ApiKey;
            }
            private set
            {
                if (m_ApiKey == value)
                {
                    return;
                }
                m_ApiKey = value;
            }
        }

        private static string m_ConsumerKey;
        public static string ConsumerKey
        {
            get
            {
                return m_ConsumerKey;
            }
            private set
            {
                if (m_ConsumerKey == value)
                {
                    return;
                }
                m_ConsumerKey = value;
            }
        }

        private static string m_ConsumerKeySecret;
        public static string ConsumerKeySecret
        {
            get
            {
                return m_ConsumerKeySecret;
            }
            private set
            {
                if (m_ConsumerKeySecret == value)
                {
                    return;
                }
                m_ConsumerKeySecret = value;
            }
        }

        #endregion


        public class StoredCredentials
        {
            public string OAuthRequestToken {get; set;}
            public string OAuthRequestTokenSecret {get; set;}
            public string OAuthAccessToken {get; set;}
            public string OAuthAccessTokenSecret { get; set; }
            public string ScreenName { get; set; }
        }

        private StoredCredentials m_Stored;
        private StoredCredentials Stored
        {
            get
            {
                return m_Stored;
            }
            set
            {
                if (m_Stored == value)
                {
                    return;
                }
                m_Stored = value;
            }
        }


        private static Account instance;

        private Account()
        {
            // Load stored twitter credentials if we have them
            if (IsolatedStorageSettings.ApplicationSettings.Contains(kTwitterCredential))
            {
                Stored = (StoredCredentials)IsolatedStorageSettings.ApplicationSettings[kTwitterCredential];
            }

            if (Stored == null)
            {
                Stored = new StoredCredentials();
            }

            // Load configuration information

            try
            {
                if (ApiKey == null)
                {
                    ApiKey = SecretKeys.Api;
                }

                if (ConsumerKey == null)
                {
                    ConsumerKey = SecretKeys.Consumer;
                }

                if (ConsumerKeySecret == null)
                {
                    ConsumerKeySecret = SecretKeys.ConsumerSecret;
                }

                if (CallbackUrl == null)
                {
                    CallbackUrl = SecretKeys.CallbackUrl;
                }
            }
            catch (Exception) { }

            if ((ApiKey == null) || (ConsumerKey == null) || (ConsumerKeySecret == null) || (CallbackUrl == null))
            {
                throw new Exception("Twitter: Invalid configuration file");
            }
        }

        public static Account Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Account();
                }
                return instance;
            }
        }

        public void Reset(Action<bool> done)
        {
            Logout(delegate(bool didLogOut)
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(kTwitterCredential))
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove(kTwitterCredential);
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }

                instance = new Account();

                done(didLogOut);
            });
        }

        internal class RequestContainer
        {
            public HttpWebRequest Request;
            public byte[] RequestData;
            public byte[] ResponseData;
            public string ErrorMessage;
        }

        internal void CheckCredentials(Action<bool> cb)
        {
            if ((OAuthAccessToken != null) && (OAuthAccessTokenSecret != null))
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine("Stored Twitter Credentials Found");
                }

                // Acquire a request token
                Dictionary<string, string> oAuthParams = new Dictionary<string, string>() { 
                                                                   {RequestKeys.kOAuthConsumerKey, Account.ConsumerKey},
                                                                   {RequestKeys.kOAuthNonce, Helpers.GenerateNonce()},
                                                                   {RequestKeys.kOAuthSignatureMethod, ApiUrl.OAuthSignatureMethod},
                                                                   {RequestKeys.kOAuthToken, OAuthAccessToken},
                                                                   {RequestKeys.kOAuthTimeStamp, Helpers.TimeStamp()},
                                                                   {RequestKeys.kOAuthVersion, RequestKeys.kOAuthVersionValue}
                };

                // Create a string of normalized paramaters
                string normalizedParams = Helpers.NormalizeAllParams(oAuthParams, null);

                //Build the signature base string
                string signatureBase = Helpers.GetSignatureBase(Helpers.HttpMethod.Get, ApiUrl.VerifyCredentialsUrl, normalizedParams);

                string signature = Helpers.GetSignature(signatureBase, Account.ConsumerKeySecret, OAuthAccessTokenSecret);

                oAuthParams.Add(RequestKeys.kOAuthSignature, signature);

                StringBuilder authorizeHeader = AuthorizeHeaderFromKeyValues(oAuthParams);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiUrl.VerifyCredentialsUrl);

                request.Headers["Authorization"] = authorizeHeader.ToString() + "\n";
                request.Method = "GET";
                request.AllowReadStreamBuffering = true; 

                RequestContainer requestContainer = new RequestContainer();

                requestContainer.Request = request;

                SendRequest(requestContainer, delegate(RequestContainer req)
                {
                    if (req.ErrorMessage != null)
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine("Stored Twitter Credentials are invalid");
                        }

                        cb(false);
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine("Stored Twitter Credentials are valid");
                        }


                        cb(true);
                    }
                });
            }
            else
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine("No Stored Twitter credentials found");
                }

                cb(false);
            }
        }

        public void Logout(Action<bool> cb)
        {
            if ((OAuthAccessToken != null) && (OAuthAccessTokenSecret != null))
            {
                string baseUrl = ApiUrl.EndSessionUrl;
                Dictionary<string, string> queryParams = new Dictionary<string, string>() {
                                                        {RequestKeys.kOAuthCallback, Account.CallbackUrl}
                };

                // Acquire a request token
                Dictionary<string, string> oAuthParams = new Dictionary<string, string>() { 
                                                                   {RequestKeys.kOAuthConsumerKey, Account.ConsumerKey},
                                                                   {RequestKeys.kOAuthNonce, Helpers.GenerateNonce()},
                                                                   {RequestKeys.kOAuthSignatureMethod, ApiUrl.OAuthSignatureMethod},
                                                                   {RequestKeys.kOAuthToken, OAuthAccessToken},
                                                                   {RequestKeys.kOAuthTimeStamp, Helpers.TimeStamp()},
                                                                   {RequestKeys.kOAuthVersion, RequestKeys.kOAuthVersionValue}
                };

                // Create a string of normalized paramaters
                string normalizedParams = Helpers.NormalizeAllParams(oAuthParams, queryParams);

                //Build the signature base string
                string signatureBase = Helpers.GetSignatureBase(Helpers.HttpMethod.Post, baseUrl, normalizedParams);

                string signature = Helpers.GetSignature(signatureBase, Account.ConsumerKeySecret, OAuthAccessTokenSecret);

                oAuthParams.Add(RequestKeys.kOAuthSignature, signature);

                StringBuilder authorizeHeader = AuthorizeHeaderFromKeyValues(oAuthParams);

                StringBuilder postBody = PostDataFromKeyValues(queryParams);

                string url = CreateFullUrl(baseUrl, null);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                byte[] postBytes = Encoding.UTF8.GetBytes(postBody.ToString());

                request.Headers["Authorization"] = authorizeHeader.ToString() + "\n";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.AllowReadStreamBuffering = true;

                RequestContainer requestContainer = new RequestContainer();

                requestContainer.Request = request;
                requestContainer.RequestData = postBytes;

                SendRequest(requestContainer, delegate(RequestContainer req)
                {
                    if (req.ErrorMessage != null)
                    {
                        cb(false);
                    }
                    else
                    {
                        // TODO: At some point we may want to parse the XML response here
                        string s = Encoding.UTF8.GetString(req.ResponseData, 0, req.ResponseData.Length);

                        cb(true);
                    }
                });
            }
            else
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine("No Stored Twitter credentials found");
                }

                cb(false);
            }
        }

        public void VerifyCredentials(Action<string, string> needsUserAuthorization)
        {
            CheckCredentials(delegate(bool credentialsOk)
            {
                if (credentialsOk)
                {
                    needsUserAuthorization(null, null);
                    return;
                }
                else
                {
                    OAuthAccessToken = null;
                    OAuthAccessTokenSecret = null;
                    OAuthRequestToken = null;
                    OAuthRequestTokenSecret = null;

                    string baseUrl = ApiUrl.RequestTokenUrl;
                    Dictionary<string, string> queryParams = new Dictionary<string, string>() {
                                                        {RequestKeys.kOAuthCallback, Account.CallbackUrl}
                    };

                    // Acquire a request token
                    Dictionary<string, string> oAuthParams = new Dictionary<string, string>() { 
                                                                {RequestKeys.kOAuthConsumerKey, Account.ConsumerKey},
                                                                {RequestKeys.kOAuthNonce, Helpers.GenerateNonce()},
                                                                {RequestKeys.kOAuthSignatureMethod, ApiUrl.OAuthSignatureMethod},
                                                                {RequestKeys.kOAuthTimeStamp, Helpers.TimeStamp()},
                                                                {RequestKeys.kOAuthVersion, RequestKeys.kOAuthVersionValue}
                    };

                    // Create a string of normalized paramaters
                    string normalizedParams = Helpers.NormalizeAllParams(oAuthParams, queryParams);

                    //Build the signature base string
                    string signatureBase = Helpers.GetSignatureBase(Helpers.HttpMethod.Get, baseUrl, normalizedParams);

                    string signature = Helpers.GetSignature(signatureBase, Account.ConsumerKeySecret, null);

                    oAuthParams.Add(RequestKeys.kOAuthSignature, signature);

                    StringBuilder authorizeHeader = AuthorizeHeaderFromKeyValues(oAuthParams);


                    string url = CreateFullUrl(baseUrl, queryParams);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                    request.Headers["Authorization"] = authorizeHeader.ToString() + "\n";
                    request.Method = "GET";
                    request.AllowReadStreamBuffering = true;

                    RequestContainer requestContainer = new RequestContainer();

                    requestContainer.Request = request;

                    SendRequest(requestContainer, delegate(RequestContainer req)
                    {
                        if (req.ErrorMessage != null)
                        {
                            needsUserAuthorization(null, req.ErrorMessage);
                        }
                        else
                        {
                            Dictionary<string, string> kVMap = ParseResultParams(req.ResponseData);

                            string token = kVMap.ContainsKey(RequestKeys.kOAuthToken) == false ? null : kVMap[RequestKeys.kOAuthToken];
                            string tokenSecret = kVMap.ContainsKey(RequestKeys.kOAuthTokenSecret) == false ? null : kVMap[RequestKeys.kOAuthTokenSecret];

                            if ((null == token) || (null == tokenSecret))
                            {
                                throw new Exception("Invalid twitter response");
                            }

                            OAuthRequestToken = token;
                            OAuthRequestTokenSecret = tokenSecret;

                            needsUserAuthorization(ApiUrl.AuthenticationUrl(OAuthRequestToken), null);
                        }
                    });
                }
            });
        }

        public void UpdateStatus(string status, Action<string> completed)
        {
            if ((OAuthAccessToken == null) || (OAuthAccessTokenSecret == null))
            {
                completed("Not Authorized to update this Twitter Account");
            }
            else
            {
                string baseUrl = ApiUrl.StatusUpdateUrl;
                Dictionary<string, string> queryParams = new Dictionary<string, string>() {
                    {RequestKeys.kStatus, status}
                };

                // Acquire a request token
                Dictionary<string, string> oAuthParams = new Dictionary<string, string>() { 
                                                                   {RequestKeys.kOAuthConsumerKey, Account.ConsumerKey},
                                                                   {RequestKeys.kOAuthNonce, Helpers.GenerateNonce()},
                                                                   {RequestKeys.kOAuthSignatureMethod, ApiUrl.OAuthSignatureMethod},
                                                                   {RequestKeys.kOAuthToken, OAuthAccessToken},
                                                                   {RequestKeys.kOAuthTimeStamp, Helpers.TimeStamp()},
                                                                   {RequestKeys.kOAuthVersion, RequestKeys.kOAuthVersionValue}
                };

                // Create a string of normalized paramaters
                string normalizedParams = Helpers.NormalizeAllParams(oAuthParams, queryParams);

                //Build the signature base string
                string signatureBase = Helpers.GetSignatureBase(Helpers.HttpMethod.Post, baseUrl, normalizedParams);

                string signature = Helpers.GetSignature(signatureBase, Account.ConsumerKeySecret, OAuthAccessTokenSecret);

                oAuthParams.Add(RequestKeys.kOAuthSignature, signature);

                StringBuilder authorizeHeader = AuthorizeHeaderFromKeyValues(oAuthParams);

                StringBuilder postBody = PostDataFromKeyValues(queryParams);

                string url = CreateFullUrl(baseUrl, null);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                byte[] postBytes = Encoding.UTF8.GetBytes(postBody.ToString());

                request.Headers["Authorization"] = authorizeHeader.ToString() + "\n";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.AllowReadStreamBuffering = true; 

                RequestContainer requestContainer = new RequestContainer();

                requestContainer.Request = request;
                requestContainer.RequestData = postBytes;

                SendRequest(requestContainer, delegate(RequestContainer req)
                {
                    if (req.ErrorMessage != null)
                    {
                        completed( req.ErrorMessage);
                    }
                    else
                    {
                        // TODO: At some point we may want to parse the XML response here
                        string s = Encoding.UTF8.GetString(req.ResponseData,0, req.ResponseData.Length);

                        completed(null);
                    }
                });
            }
        }

        private static StringBuilder AuthorizeHeaderFromKeyValues(Dictionary<string, string> oAuthParams)
        {
            StringBuilder authorizeHeader = new StringBuilder("OAuth ");

            int i = 0;

            foreach (string k in oAuthParams.Keys)
            {
                i++;
                authorizeHeader.Append(Helpers.UriEscape(k));
                authorizeHeader.Append("=\"");
                authorizeHeader.Append(Helpers.UriEscape(oAuthParams[k]));

                if (i < oAuthParams.Keys.Count)
                {
                    authorizeHeader.Append("\",");
                }
                else
                {
                    authorizeHeader.Append("\"");
                }
            }
            return authorizeHeader;
        }

        private static StringBuilder PostDataFromKeyValues(Dictionary<string, string> queryParams)
        {
            StringBuilder postBody = new StringBuilder();

            int i = 0;
            foreach (string k in queryParams.Keys)
            {
                i++;
                postBody.Append(Helpers.UriEscape(k));
                postBody.Append("=");
                postBody.Append(Helpers.UriEscape(queryParams[k]));

                if (i < queryParams.Keys.Count)
                {
                    postBody.Append(",");
                }
            }
            return postBody;
        }

        Dictionary<string, string> ParseResultParams(byte[] bytes)
        {
            string s = UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            Dictionary<string, string> kVMap = new Dictionary<string, string>();

            string[] pairs = s.Split('&', '=');

            if (pairs.Length % 2 != 0)
            {
                throw new Exception("Invalid params in result");
            }

            for (int i = 0; i < pairs.Length; i += 2)
            {
                kVMap.Add(pairs[i], pairs[i + 1]);
            }

            return kVMap;
        }

        internal string CreateFullUrl(string p, Dictionary<string, string> queryParams)
        {
            if ((queryParams == null) || (queryParams.Count == 0))
            {
                return p;
            }

            StringBuilder sb = new StringBuilder(p);

            int i = 0;

            foreach (string k in queryParams.Keys)
            {
                if (i == 0)
                {
                    sb.Append("?");
                }
                else
                {
                    sb.Append("&");
                }

                i++;

                sb.Append(Helpers.UriEscape(k));
                sb.Append("=");
                sb.Append(Helpers.UriEscape(queryParams[k]));
            }

            return sb.ToString();
        }

        internal void SendRequest(RequestContainer requestContainer, Action<RequestContainer> requestCompleted)
        {
            // Note that current design assumes a successfull post will be followed by a getAndRead
            AsyncCallback getAndRead = new AsyncCallback(delegate(IAsyncResult responseResult)
            {
                RequestContainer responseContainer = (RequestContainer)responseResult.AsyncState;

                try
                {
                    HttpWebResponse response = (HttpWebResponse)responseContainer.Request.EndGetResponse(responseResult);

                    Stream s = response.GetResponseStream();

                    byte[] bytes = new byte[s.Length];
                    s.Read(bytes, 0, (int)s.Length);

                    // Set the response
                    responseContainer.ResponseData = bytes;

                    // Release the HttpWebResponse
                    response.Close();
                }
                catch (WebException we)
                {
                    responseContainer.ErrorMessage = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                }
                catch (Exception e)
                {
                    responseContainer.ErrorMessage = e.ToString();
                }
                finally
                {
                    requestCompleted(requestContainer);
                }
            });

            AsyncCallback postAndWrite = new AsyncCallback(delegate(IAsyncResult ar)
            {
                RequestContainer requestCBContainer = (RequestContainer)ar.AsyncState;

                try
                {
                    HttpWebRequest request = requestCBContainer.Request;

                    if (requestCBContainer != null)
                    {

                    byte[] postBytes = requestCBContainer.RequestData;

                    Stream s = request.EndGetRequestStream(ar);

                    s.Write(postBytes, 0, postBytes.Length);
                    s.Close();
                    }
                    // Start the asynchronous operation to get the response

                    request.BeginGetResponse(getAndRead,
                        requestCBContainer
                    );
                }
                catch (WebException we)
                {
                    requestCBContainer.ErrorMessage = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                    requestCompleted(requestContainer);
                }
                catch (Exception e)
                {
                    requestCBContainer.ErrorMessage = e.ToString();
                    requestCompleted(requestContainer);
                }
            });

            try
            {
                if ("POST" == requestContainer.Request.Method)
                {
                    requestContainer.Request.ContentType = "application/x-www-form-urlencoded";
                    requestContainer.Request.BeginGetRequestStream(postAndWrite, requestContainer);
                }
                else
                {
                    requestContainer.Request.BeginGetResponse(getAndRead, requestContainer);
                }
            }
            catch (Exception e)
            {
                requestContainer.ErrorMessage = e.ToString();
                requestCompleted(requestContainer);
            }
        }

        public void CompleteAuthorization(string webBrowserRedirectUrl, Action<string> authorizationCompleted)
        {
            Dictionary<string, string> qParams = Helpers.ParseQueryString(webBrowserRedirectUrl);

            string oAuthToken = null;
            string oAuthVerifier = null;

            if (qParams.ContainsKey("oauth_token"))
            {
                oAuthToken = qParams["oauth_token"];
            }

            if (qParams.ContainsKey("oauth_verifier"))
            {
                oAuthVerifier = qParams["oauth_verifier"];
            }

            if ((oAuthToken == null) || (oAuthVerifier == null))
            {
                string error = "Invalid browser redirect URL";
                authorizationCompleted(error);
                throw new Exception(error);
            }

            string baseUrl = ApiUrl.AccessTokenUrl;

            //Dictionary<string, string> queryParams = null;
             //RMM may not need this
            Dictionary<string, string> queryParams = new Dictionary<string, string>() {
                {RequestKeys.kOAuthVerifier, oAuthVerifier},
            };

            // Acquire an access token
            Dictionary<string, string> oAuthParams = new Dictionary<string, string>() { 
                                                                {RequestKeys.kOAuthConsumerKey, Account.ConsumerKey},
                                                                {RequestKeys.kOAuthNonce, Helpers.GenerateNonce()},
                                                                {RequestKeys.kOAuthSignatureMethod, ApiUrl.OAuthSignatureMethod},
                                                                {RequestKeys.kOAuthTimeStamp, Helpers.TimeStamp()},
                                                                {RequestKeys.kOAuthVersion, RequestKeys.kOAuthVersionValue},
                                                                {RequestKeys.kOAuthToken, oAuthToken}
            };

            // Create a string of normalized paramaters
            string normalizedParams = Helpers.NormalizeAllParams(oAuthParams, queryParams);

            //Build the signature base string
            string signatureBase = Helpers.GetSignatureBase(Helpers.HttpMethod.Get, baseUrl, normalizedParams);

            string signature = Helpers.GetSignature(signatureBase, Account.ConsumerKeySecret, OAuthRequestTokenSecret);

            oAuthParams.Add(RequestKeys.kOAuthSignature, signature);

            StringBuilder authorizeHeader = AuthorizeHeaderFromKeyValues(oAuthParams);

            string url = CreateFullUrl(baseUrl, queryParams);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers["Authorization"] = authorizeHeader.ToString() + "\n";
            request.Method = "GET";
            request.AllowReadStreamBuffering = true; 

            RequestContainer requestContainer = new RequestContainer();

            requestContainer.Request = request;

            SendRequest(requestContainer, delegate(RequestContainer req)
            {
                if (req.ErrorMessage != null)
                {
                    authorizationCompleted(req.ErrorMessage);
                }
                else
                {
                    Dictionary<string, string> kVMap = ParseResultParams(req.ResponseData);

                    string token = kVMap.ContainsKey(RequestKeys.kOAuthToken) == false ? null : kVMap[RequestKeys.kOAuthToken];
                    string tokenSecret = kVMap.ContainsKey(RequestKeys.kOAuthTokenSecret) == false ? null : kVMap[RequestKeys.kOAuthTokenSecret];
                    string screenName = kVMap.ContainsKey(RequestKeys.kScreenName) == false ? null : kVMap[RequestKeys.kScreenName];

                    if ((null == token) || (null == tokenSecret) || (null == screenName))
                    {
                        throw new Exception("Invalid twitter response");
                    }

                    OAuthAccessToken = token;
                    OAuthAccessTokenSecret = tokenSecret;
                    ScreenName = screenName;

                    Save();

                    authorizationCompleted(null);
                }
            });
        }

        internal void Save()
        {
            IsolatedStorageSettings.ApplicationSettings[kTwitterCredential] = m_Stored;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
