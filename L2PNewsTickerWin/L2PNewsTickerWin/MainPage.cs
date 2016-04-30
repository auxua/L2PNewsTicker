using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using L2PAPIClientPortable;

using Xamarin.Forms;
using System.Diagnostics;
using System.Windows.Input;


namespace L2PNewsTickerWin
{
    
    public class MainPage : ContentPage
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
                //Device.BeginInvokeOnMainThread(() => label.Text += text + Environment.NewLine);
                Device.BeginInvokeOnMainThread(() => label.Text = text);
            }

            public void ImportantLog(string text)
            {
                Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Important", text, "OK"));
            }
        }

        private Label label;
		private Button AuthorizeButton;
        private Button RefreshButton;
        private Button StartUpdateButton;
        private ListView list;
        private ProgressBar bar;

        private bool isAuthorized = false;

        /*
        * Colors of the UI are defined here - maybe by that it will be configurable later...    
        */

#if OLDCOLORS
        public static Color Background = Color.FromHex("407FB7");
        public static Color FontColor = Color.FromHex("ECEDED");
        public static Color SecondaryColor = Color.FromHex("000000");
#elif WINDOWS_PHONE
        public static Color Background = Color.FromHex("000000");
        public static Color FontColor = Color.FromHex("F8F8F8");
        public static Color SecondaryColor = Color.FromHex("87CEFA");
#elif ANDROID
        public static Color Background = Color.FromHex("000000");
        public static Color FontColor = Color.FromHex("F8F8F8");
        //public static Color SecondaryColor = Color.FromHex("F0FFFF");
        public static Color SecondaryColor = Color.FromHex("87CEFA");
#else
        public static Color Background = Color.FromHex("FFFFFF");
        public static Color FontColor = Color.FromHex("111111");
        public static Color SecondaryColor = Color.FromHex("0000CD");
#endif
        /*public static Color Background = Color.FromHex("EDEDED");
        public static Color FontColor = Color.FromHex("4169E1");
        public static Color SecondaryColor = Color.FromHex("000000");*/

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
                    //AuthorizeButton.IsVisible = !value;
                    AuthorizeButton.IsEnabled = !value;
                    RefreshButton.IsEnabled = value;
                    //StartUpdateButton.IsVisible = value;
                });
                //if (!value) return;
                // start work directly
                //this.GetCourseUpdates(this, null);
            }
        }

        private bool needsUpdate = false;

        public bool NeedsUpdate
        {
            get
            {
                return this.needsUpdate;
            }
            set
            {
                this.needsUpdate = value;
                Device.BeginInvokeOnMainThread(() =>
                {
                    //AuthorizeButton.IsVisible = !value;
                    AuthorizeButton.IsEnabled = !value;
                    RefreshButton.IsEnabled = value;
                    //StartUpdateButton.IsVisible = value;
                });
                if (!value) return;
                // start work directly
                this.GetCourseUpdates(this, null);
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
                    //bar.IsVisible = value;
                    if (!value)
                    {
                        RefreshButton.Text = Localization.Localize("Refresh");
                    }
                    else
                    {
                        RefreshButton.Text = Localization.Localize("Cancel");
                    }
                });
                //if (!value) return;
                // start work directly
                //this.Button2_Clicked(this, null);
            }
        }

        private ICommand getConfigPage;

        public ICommand GetConfigPage
        {
            get
            {
                return getConfigPage;
            }
        }

        private async Task StateWrapper()
        {

            bool auth;

            // Data-based approach:
            //  Try getting Data from DataManager. If there is Data, assume authorization
            DataManager.Load();
            auth = DataManager.hasData();

            /*
            // Naive approach - only works if there is Internet Conenction
            await L2PAPIClient.AuthenticationManager.CheckAccessTokenAsync();
            auth = (L2PAPIClient.AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE);
            */
            IsAuthorized = auth;
            // Try Loading the old values 
            //if (DataManager.Load())
            if (auth)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    list.ItemsSource = DataManager.GetCoursesCellAdaption();
                    list.IsEnabled = true;
                });
            }
        }

        public MainPage ()
		{
            Title = "L2P Newsticker";

            // Is User already authorized?
            /*L2PAPIClient.AuthenticationManager.CheckAccessTokenAsync();
            bool auth = (L2PAPIClient.AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE);*/

            Task.Factory.StartNew(async () => await StateWrapper());

            label = new Label();
            label.Text = Localization.Localize("Ready");
            label.TextColor = FontColor;
            label.HorizontalOptions = LayoutOptions.Center;

			AuthorizeButton = new Button();
			AuthorizeButton.Text = Localization.Localize("Authorize");
			AuthorizeButton.Clicked += Button_Clicked;
			if (Device.OS != TargetPlatform.iOS)
            	AuthorizeButton.TextColor = FontColor;
            AuthorizeButton.HorizontalOptions = LayoutOptions.FillAndExpand;

            /*StartUpdateButton = new Button();
            StartUpdateButton.Text = "Start query";
            StartUpdateButton.Clicked += GetCourseUpdates;*/

            RefreshButton = new Button();
            RefreshButton.Text = Localization.Localize("Refresh");
			if (Device.OS != TargetPlatform.iOS)
            	RefreshButton.TextColor = FontColor;
            RefreshButton.Clicked += GetCourseUpdates;
            RefreshButton.IsEnabled = false;
            RefreshButton.HorizontalOptions = LayoutOptions.FillAndExpand;

            var line = new BoxView();
            line.HeightRequest = 5;
            //line.Color = FontColor;
            line.Color = Color.FromRgb(0, 84, 159);
            //line.HorizontalOptions = LayoutOptions.CenterAndExpand;
            //line.TranslationX = -10;
            line.WidthRequest = 1080; // just use full width of screen...

            StackLayout buttons = new StackLayout
            {
                Children =
                {
                    AuthorizeButton,
                    RefreshButton
                },
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Background
            };

            list = new ListView();
            list.ItemTapped += TappedCourse;
            list.HasUnevenRows = true;
			if (Device.OS == TargetPlatform.iOS)
				list.BackgroundColor = MainPage.Background;
				
            /*var customCell = new DataTemplate(typeof(TextCell));
            customCell.SetBinding(TextCell.TextProperty, "Text");
            customCell.SetBinding(TextCell.DetailProperty, "Detail");
            customCell.SetBinding(TextCell.TextColorProperty, "MainColor");
            customCell.SetBinding(TextCell.DetailColorProperty, "DetailColor");*/
            var customCell = new DataTemplate(typeof(CourseInfoCell));
            list.ItemTemplate = customCell;

            bar = new ProgressBar();
            bar.BackgroundColor = FontColor;
            bar.HorizontalOptions = LayoutOptions.Fill;

            TestPageLoggingAdapter Logger = new TestPageLoggingAdapter(label);
            DataManager.setLogger(Logger);

            ScrollView scroll = new ScrollView();
            //scroll.Content = new StackLayout

            StackLayout stack = new StackLayout
            {
                Children =
                {
                    //AuthorizeButton,
                    //StartUpdateButton,
                    buttons,
                    bar,
//#if DEBUG
                    label,
                    line,
//#endif
                    list
                },
                BackgroundColor = Background,
            };
            stack.Padding = new Thickness(10, 10);
            stack.HorizontalOptions = LayoutOptions.Fill;
            scroll.Content = stack;

            //Content = scroll;
            Content = stack;

            getConfigPage = new Command(() =>
            {
                ConfigPage page = new ConfigPage();
                Device.BeginInvokeOnMainThread(() => Navigation.PushAsync(page));
            });

            ToolbarItem tb = new ToolbarItem();
            tb.Text = Localization.Localize("Config");
            tb.Command = GetConfigPage;
            tb.Icon = String.Format("{0}{1}.png", Device.OnPlatform("", "", "Assets/"), "settings");
            ToolbarItems.Add(tb);

            // Toggle Visibility for Authorization Status
            //IsAuthorized = auth;
            //isAuthorized = auth;
        }

        

        private void TappedCourse(object sender, ItemTappedEventArgs e)
        {
            //throw new NotImplementedException();
            CourseCellAdapter source = (CourseCellAdapter)e.Item;
            if (source == null) return;

            IsBusy = true;

            //var data = DataManager.GetCourseDataElements(source.cid);
            var data = DataManager.GetCourseDataElementsFlat(source.cid);
            CourseDetailPage page = new CourseDetailPage(data);

            IsBusy = false;
            if (data.Count>0)
                Device.BeginInvokeOnMainThread(() => Navigation.PushAsync(page));
            else
                Device.BeginInvokeOnMainThread(() => DisplayAlert("No Data", Localization.Localize("NoData"),"OK"));

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
                    ((MainPage)outerPage).IsGettingData = false;
                    //outerPage.DisplayAlert("Finsihed", "Finished work", "OK");
                    //list.ItemsSource = DataManager.GetUpdateStringsAsList();
                    list.ItemsSource = DataManager.GetCoursesCellAdaption();
                    list.IsEnabled = true;
                });
                //DataManager.ShowUpdatesViaLog();
                // Save the data to local persistent storage
                //DataManager.Store();
                DataManager.StoreAsTask();

            }

            public void beforeGettingCourses(int cids)
            {
                // set maxValue to cid+1 to allow progress indication for getting courses
                maxValue = cids+1;
                stepSize = 1.0 / maxValue;
                Device.BeginInvokeOnMainThread(() =>
                {
                    bar.ProgressTo((++step)*stepSize,250,Easing.Linear);
                    // prevent user from tapping List to avoid inconsistent Data queries
                    list.IsEnabled = false;
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

        private async void GetCourseUpdates(object sender, EventArgs e)
        {
            if (this.IsGettingData)
            {
                // Already working, so abort is requested by user
                DataManager.CancelUpdate();
                Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Cancel", Localization.Localize("CancelInfo"), "OK");
                        // Just avoid the bar to hang somewhere in between
                        bar.ProgressTo(0, 250, Easing.Linear);
                        IsBusy = false;
                        IsGettingData = false;
                        // enable this list in case, there is data to show and tap
                        list.IsEnabled = DataManager.hasData();
                    });
                
                return;
            }

            this.IsGettingData = true;
            IsBusy = true;

            try
            {
                // Create Callback and start Work
                await DataManager.startUpdateNew(new SimpleFinishedCallBack(this, list, bar));
            }
            catch (AuthenticationManager.NotAuthorizedException)
            {
                // handle Authorization Issue
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.IsAuthorized = false;
                    this.IsBusy = false;
                    this.IsGettingData = false;
                    bar.ProgressTo(0, 250, Easing.BounceOut);
                    label.Text = Localization.Localize("Ready");
                    DisplayAlert("Problem", Localization.Localize("AuthorizationProblem"), "OK");
                });
                return;
            }
            catch (DataManager.NoCoursesException)
            {
                // handle problem of lack of Courses
                Device.BeginInvokeOnMainThread(() =>
                {
                    //this.IsAuthorized = false;
                    this.IsBusy = false;
                    this.IsGettingData = false;
                    bar.ProgressTo(0, 250, Easing.BounceOut);
                    label.Text = Localization.Localize("Ready");
                    DisplayAlert("Problem", Localization.Localize("NoCoursesError"), "OK");
                });
                return;
            }
            catch // Otherwise, just Internet Problems
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    label.Text = "Error (Internet Problem)";
                    IsBusy = false;
                    IsGettingData = false;
                    bar.ProgressTo(0, 250, Easing.BounceOut);
                });
                
            }
        }

        /*public static bool HasInternet()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            return (connectionProfile != null &&
                    connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }*/

        async void Button_Clicked (object sender, EventArgs e)
		{


            try
            {

/*#if (__ANDROID__ || __IOS__)
                // nothing
#else
                if (!Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    throw new Exception();
                if (!HasInternet())
                    throw new Exception();
#endif*/

                //label.Text = "Starting";
                label.Text = Localization.Localize("Starting");

				Device.BeginInvokeOnMainThread(() => AuthorizeButton.IsEnabled = false);

                string url;
                try
                {
                    url = await L2PAPIClientPortable.AuthenticationManager.StartAuthenticationProcessAsync();
                    if (string.IsNullOrEmpty(url))
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    throw new System.Net.WebException();
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Authorization", Localization.Localize("UserAuthorizationInformation"), "OK");
                    Device.OpenUri(new Uri(url));
                });

                

				bool auth = false;

                // just try to check authorize sometimes in case of slow device
                for (int i = 0; i < 12; i++)
                {
                    //Thread.Sleep(5000);
                    await Task.Delay(5000);
                    auth = await L2PAPIClientPortable.AuthenticationManager.CheckAuthenticationProgressAsync();
                    if (auth)
                    {
                        label.Text = Localization.Localize("Done");
                        break;
                    }
                }
				if (!auth)
				{
					// Did not succeed
					throw new Exception();
				}
            }
            catch (System.Net.WebException ex)
            {
                label.Text = Localization.Localize("NoInternet");
            }
			catch (Exception ex)
			{
				// Did not succeed
				Device.BeginInvokeOnMainThread(() => {
					AuthorizeButton.IsEnabled = false;
					label.Text = "Timeout "+Localization.Localize("Ready");
				});
			}
            finally
            {
                // Check Status
                IsAuthorized = (L2PAPIClientPortable.AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE);
                if (IsAuthorized) NeedsUpdate = true;
            }
            
        }
	}
}
