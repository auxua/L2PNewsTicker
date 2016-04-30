using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using Xamarin.Forms;
using System.Reflection;


namespace L2PNewsTicker
{
    public class Localization
    {
        /// <remarks>
        /// Maybe we can cache this info rather than querying every time
        /// </remarks>
        public static string Locale()
        {
            return DependencyService.Get<ILocale>().GetCurrent();
        }

        public static string Localize(string key)
        {

            // Workaround: WP does not localize with default Localization, so use it hard-coded
#if WINDOWS_PHONE
            //var prop = typeof(L2PNewsTicker.WinPhone.Resources.AppResources).GetProperty(key);
            //string value = (string)prop.GetValue(L2PNewsTicker.WinPhone.Resources.AppResources.,null);

            var man = L2PNewsTicker.WinPhone.Resources.AppResources.ResourceManager;
            //var c = WinPhone.Resources.AppResources.Culture;
            //return WinPhone.Resources.AppResources.Ready;
            
            var c = new WinPhone.Locale_WinPhone();
            var current = c.GetCurrent();

            if (current.Contains("de"))
                return man.GetString(key+"DE");
            else
                return man.GetString(key);
            
#endif

#if DEBUG
            // FOR DEBUGGING
            var assembly = typeof(Localization).GetTypeInfo().Assembly;
            foreach (var res in assembly.GetManifestResourceNames())
                System.Diagnostics.Debug.WriteLine("found resource: " + res);
#endif
            var netLanguage = Locale();

            ResourceManager temp;

            /*
			 * Workaround!
			 * 
			 * On Visual Studio Build, use the upper line,
			 * using Xamarin on OS X user the second line (with ResourceFiles.)
			 * 
			 * This is a Bug in Xamarin and will be fixed later... hopefully
			 */

            //temp = new ResourceManager("L2PNewsTicker." + ProjectInfix() + ".L2PNewsTickerResources", typeof(Localization).GetTypeInfo().Assembly);

            //temp = new ResourceManager("L2PNewsTicker." + ProjectInfix() + ".L2PNewsTickerResources", typeof(Localization).GetTypeInfo().Assembly);
            temp = new ResourceManager("L2PNewsTicker." + ProjectInfix() + ".L2PNewsTickerResources", typeof(Localization).GetTypeInfo().Assembly);

            //ResourceManager temp = new ResourceManager("MensaApp.MensaAppResources", typeof(Localization).GetTypeInfo().Assembly);
            /*if (Device.OS != TargetPlatform.iOS)
            {
                temp = new ResourceManager("MensaApp." + ProjectInfix() + ".MensaAppResources", typeof(Localization).GetTypeInfo().Assembly);
            } 
            else
            {
                temp = new ResourceManager("MensaApp." + ProjectInfix() + ".ResourceFiles.MensaAppResources", typeof(Localization).GetTypeInfo().Assembly);
            }*/
            //ResourceManager temp = new ResourceManager("MensaApp."+ProjectInfix()+".MensaAppResources", typeof(Localization).GetTypeInfo().Assembly);

            var cult = new CultureInfo(netLanguage);
            //var cultDE = new CultureInfo("de");

            //cult = cultDE;

            string result = temp.GetString(key, cult);

            return result;
        }

        public static string ProjectInfix()
        {
            if (Device.OS == TargetPlatform.Android)
                return "Droid";
            if (Device.OS == TargetPlatform.iOS)
                return "iOS";
            return "WinPhone";
        }
    }
}
