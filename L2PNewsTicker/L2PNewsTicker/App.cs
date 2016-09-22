//#define RESET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace L2PNewsTicker
{
	public class App : Application
	{
        public static string version = "1.2.1";

        public App ()
		{
            
#if RESET
			Application.Current.Properties.Clear();
#endif
			// The root page of your application
            MainPage = new NavigationPage(new MainPage());
            //MainPage = new MainPage();

		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
        
	}
}
