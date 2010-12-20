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

namespace Twitter
{
    public static class RequestKeys
    {
        public const string kOAuthCallback             = "oauth_callback";
        public const string kOAuthConsumerKey          = "oauth_consumer_key";
        public const string kOAuthNonce                = "oauth_nonce";
        public const string kOAuthSignatureMethod      = "oauth_signature_method";
        public const string kOAuthTimeStamp            = "oauth_timestamp";
        public const string kOAuthVersion              = "oauth_version";
        public const string kOAuthVersionValue         = "1.0";
        public static string kOAuthSignature           = "oauth_signature";
        public const string kOAuthVerifier             = "oauth_verifier";
        public const string kOAuthToken                = "oauth_token";
        public const string kOAuthTokenSecret          = "oauth_token_secret";
        public const string kOAuthCallbackConfirmed    = "oauth_callback_confirmed";
        public const string kScreenName                = "screen_name";
        public const string kStatus                    = "status";
    }
}
