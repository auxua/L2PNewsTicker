using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(L2PNewsTicker.WinPhone.Locale_WinPhone))]

namespace L2PNewsTicker.WinPhone
{
    public class Locale_WinPhone : L2PNewsTicker.ILocale
    {
        /// <remarks>
        /// Not sure if we can cache this info rather than querying every time
        /// </remarks>
        public string GetCurrent()
        {
            var lang = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            return lang;
        }
    }
}
