using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace L2PAPIClient
{
    class Config
    {
        internal const string OAuthEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/code";

        internal const string OAuthTokenEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/token";

        internal const string ClientID = "";

        internal const string L2PEndPoint = "https://www3.elearning.rwth-aachen.de/_vti_bin/l2pservices/api.svc/v1";

        internal const string OAuthTokenInfoEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/tokeninfo";

        #region Token Management (Add Storage Option in here!)

        private static string accessToken = "";

        internal static string getAccessToken()
        {
            //return accessToken;
            object o;
            if (Application.Current.Properties.TryGetValue("accessToken", out o))
                return (string)o;

            // Not found
            return "";

        }

        internal static void setAccessToken(string token)
        {
            //accessToken = token;
            Application.Current.Properties["accessToken"] = token;
        }

        private static string refreshToken = "";

        internal static string getRefreshToken()
        {
            //return refreshToken;
            object o;
            if (Application.Current.Properties.TryGetValue("refreshToken", out o))
                return (string)o;

            // Not found
            return "";
        }

        internal static void setRefreshToken(string token)
        {
            //refreshToken = token;
            Application.Current.Properties["refreshToken"] = token;
        }


        private static string deviceToken = "";

        internal static string getDeviceToken()
        {
            //return deviceToken;
            object o;
            if (Application.Current.Properties.TryGetValue("deviceToken", out o))
                return (string)o;

            // Not found
            return "";
        }

        internal static void setDeviceToken(string token)
        {
            //deviceToken = token;
            Application.Current.Properties["deviceToken"] = token;
        }


        #endregion
    }
}
