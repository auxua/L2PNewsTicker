using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;

[assembly: Dependency(typeof(L2PNewsTicker.iOS.Locale_iOS))]
namespace L2PNewsTicker.iOS
{
    public class Locale_iOS : L2PNewsTicker.ILocale
    {
        /// <remarks>
        /// Not sure if we can cache this info rather than querying every time
        /// </remarks>
        public string GetCurrent()
        {
            #region not sure why this isn't working for me (in simulator at least)
            var iosLocaleAuto = NSLocale.AutoUpdatingCurrentLocale.LocaleIdentifier;
            var iosLanguageAuto = NSLocale.AutoUpdatingCurrentLocale.LanguageCode;
            Console.WriteLine("nslocaleid:" + iosLocaleAuto);
            Console.WriteLine("nslanguage:" + iosLanguageAuto);

            var iosLocale = NSLocale.CurrentLocale.LocaleIdentifier;
            var iosLanguage = NSLocale.CurrentLocale.LanguageCode;

            var netLocale = iosLocale.Replace("_", "-");
            var netLanguage = iosLanguage.Replace("_", "-");

            Console.WriteLine("ios:" + iosLanguage + " " + iosLocale);
            Console.WriteLine("net:" + netLanguage + " " + netLocale);
            #endregion

            // HACK: not sure why NSLocale isn't ever returning correct data
            if (NSLocale.PreferredLanguages.Length > 0)
            {
                var pref = NSLocale.PreferredLanguages[0];
                netLanguage = pref.Replace("_", "-");
                Console.WriteLine("preferred:" + netLanguage);
            }
            return netLanguage;
        }
    }
}