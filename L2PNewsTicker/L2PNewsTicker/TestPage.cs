using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using L2PAPIClient;

using Xamarin.Forms;
using System.Diagnostics;

namespace L2PNewsTicker
{
    

    public class TestPage : ContentPage
	{
        public class TestPageLoggingAdapter : ILoggingAdapter
        {
            private Label label;

            public TestPageLoggingAdapter(Label l)
            {
                this.label = l;
            }

            public void Log(string text)
            {
                Device.BeginInvokeOnMainThread(() => label.Text += text + Environment.NewLine);
            }

            public void ImportantLog(string text)
            {
                Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Important", text, "OK"));
            }
        }

        private Label label;
		private Button AuthorizeButton;
        private Button StartUpdateButton;
        private ListView list;
        private ProgressBar bar;

        private bool isAuthorized = false;

        public bool IsAuthorized
        {
            get
            {
                return this.isAuthorized;
            }
            set
            {
                this.isAuthorized = value;
                Device.BeginInvokeOnMainThread(() =>
                {
                    AuthorizeButton.IsVisible = !value;
                    //StartUpdateButton.IsVisible = value;
                });
                if (!value) return;
                // start work directly
                this.Button2_Clicked(this, null);
            }
        }

        private bool isGettingData = false;

        public bool IsGettingData
        {
            get
            {
                return this.isGettingData;
            }
            set
            {
                this.isGettingData = value;
                Device.BeginInvokeOnMainThread(() =>
                {
                    //AuthorizeButton.IsVisible = !value;
                    //StartUpdateButton.IsVisible = value;
                    bar.IsVisible = value;
                });
                //if (!value) return;
                // start work directly
                //this.Button2_Clicked(this, null);
            }
        }


        public TestPage ()
		{
            // Is User already authorized?
            bool auth = (L2PAPIClient.AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE);

            label = new Label();
			label.Text = "Test";

			AuthorizeButton = new Button();
			AuthorizeButton.Text = "Start";
			AuthorizeButton.Clicked += Button_Clicked;

            StartUpdateButton = new Button();
            StartUpdateButton.Text = "Start query";
            StartUpdateButton.Clicked += Button2_Clicked;

            list = new ListView();

            bar = new ProgressBar();

            TestPageLoggingAdapter Logger = new TestPageLoggingAdapter(label);
            DataManager.setLogger(Logger);

            ScrollView scroll = new ScrollView();
            //scroll.Content = new StackLayout

            StackLayout stack = new StackLayout
            {
                Children =
                {
                    AuthorizeButton,
                    //StartUpdateButton,
                    bar,
                    label,
                    list
                }
            };
            stack.Padding = new Thickness(10, 10);
            stack.HorizontalOptions = LayoutOptions.Fill;
            scroll.Content = stack;

            Content = scroll;

            // Toggle Visibility for Authorization Status
            IsAuthorized = auth;
		}

        int cidCount;

        private class SimpleFinishedCallBack : IGetDataProgressCallBack
        {
            private Page outerPage;
            private ListView list;
            private ProgressBar bar;
            private int maxValue = 0;
            private double stepSize = 0;
            private int step = 0;

            public SimpleFinishedCallBack(Page page, ListView list, ProgressBar bar)
            {
                outerPage = page;
                this.list = list;
                this.bar = bar;
            }

            public void onCompleted()
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    outerPage.IsBusy = false;
                    //outerPage.DisplayAlert("Finsihed", "Finished work", "OK");
                    list.ItemsSource = DataManager.GetUpdateStringsAsList();
                });
                //DataManager.ShowUpdatesViaLog();
                

            }

            public void beforeGettingCourses(int cids)
            {
                // set maxValue to cid+1 to allow progress indication for getting courses
                maxValue = cids+1;
                stepSize = 1.0 / maxValue;
                Device.BeginInvokeOnMainThread(() =>
                {
                    bar.ProgressTo((++step)*stepSize,250,Easing.Linear);
                });
            }

            public void onProgress()
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    bar.ProgressTo((++step)*stepSize, 250, Easing.Linear);
                });
            }
        }

        private async void Button2_Clicked(object sender, EventArgs e)
        {
            IsBusy = true;
            
            // Create Callback and start Work
            await DataManager.startUpdate(new SimpleFinishedCallBack(this, list, bar));
        }
        
        async void Button_Clicked (object sender, EventArgs e)
		{


            try
            {

#if (__ANDROID__ || __IOS__)
                // nothing
#else
                if (!Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    throw new Exception();
#endif

                label.Text = Environment.NewLine + "start";

                string url;
                try
                {
                    url = await L2PAPIClient.AuthenticationManager.StartAuthenticationProcessAsync();
                    if (string.IsNullOrEmpty(url))
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    throw new System.Net.WebException();
                }
                
                Device.OpenUri(new Uri(url));

                // just try to check authorize sometimes in case of slow device
                for (int i = 0; i < 50; i++)
                {
                    Thread.Sleep(3000);
                    bool auth = await L2PAPIClient.AuthenticationManager.CheckAuthenticationProgressAsync();
                    if (auth)
                    {
                        label.Text = Environment.NewLine + "done";
                        break;
                    }
                }

            }
            catch (System.Net.WebException ex)
            {
                label.Text = "Error: No internet";
            }
            finally
            {
                // Check Status
                IsAuthorized = (L2PAPIClient.AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE);
            }
            
        }
	}
}
