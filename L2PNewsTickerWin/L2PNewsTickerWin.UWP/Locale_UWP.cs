using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Globalization;

[assembly: Dependency(typeof(L2PNewsTickerWin.UWP.Locale_UWP))]

namespace L2PNewsTickerWin.UWP
{
    public class Locale_UWP : L2PNewsTickerWin.ILocale
    {
        /// <remarks>
        /// Not sure if we can cache this info rather than querying every time
        /// </remarks>
        public string GetCurrent()
        {
            var culture = CultureInfo.CurrentCulture.Name;
            var lang = CultureInfo.CurrentUICulture.Name;

            //var lang = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            //var culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            return lang;
        }
    }
}
