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

        public static Color Background = Color.FromHex("407FB7");
        public static Color FontColor = Color.FromHex("ECEDED");
        public static Color SecondaryColor = Color.FromHex("000000");

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
                        RefreshButton.Text = "Refresh";
                    }
                    else
                    {
                        RefreshButton.Text = "Cancel";
                    }
                });
                //if (!value) return;
                // start work directly
                //this.Button2_Clicked(this, null);
            }
        }


        public MainPage ()
		{
            // Is User already authorized?
            bool auth = (L2PAPIClient.AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE);

            label = new Label();
			label.Text = "Ready to Start";
            label.TextColor = FontColor;
            label.HorizontalOptions = LayoutOptions.CenterAndExpand;

			AuthorizeButton = new Button();
			AuthorizeButton.Text = "Start";
			AuthorizeButton.Clicked += Button_Clicked;
            AuthorizeButton.TextColor = FontColor;
            AuthorizeButton.HorizontalOptions = LayoutOptions.FillAndExpand;

            /*StartUpdateButton = new Button();
            StartUpdateButton.Text = "Start query";
            StartUpdateButton.Clicked += GetCourseUpdates;*/

            RefreshButton = new Button();
            RefreshButton.Text = "Refresh";
            RefreshButton.TextColor = FontColor;
            RefreshButton.Clicked += GetCourseUpdates;
            RefreshButton.IsEnabled = false;
            RefreshButton.HorizontalOptions = LayoutOptions.FillAndExpand;

            var line = new BoxView();
            line.HeightRequest = 5;
            line.Color = FontColor;
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

            /*var customCell = new DataTemplate(typeof(TextCell));
            customCell.SetBinding(TextCell.TextProperty, "Text");
            customCell.SetBinding(TextCell.DetailProperty, "Detail");
            customCell.SetBinding(TextCell.TextColorProperty, "MainColor");
            customCell.SetBinding(TextCell.DetailColorProperty, "DetailColor");*/
            var customCell = new DataTemplate(typeof(CourseInfoCell));
            list.ItemTemplate = customCell;

            bar = new ProgressBar();
            bar.BackgroundColor = FontColor;

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
#if DEBUG
                    label,
                    line,
#endif
                    list
                },
                BackgroundColor = Background,
            };
            stack.Padding = new Thickness(10, 10);
            stack.HorizontalOptions = LayoutOptions.Fill;
            scroll.Content = stack;

            //Content = scroll;
            Content = stack;

            // Toggle Visibility for Authorization Status
            IsAuthorized = auth;
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
                Device.BeginInvokeOnMainThread(() => DisplayAlert("No Data","No Data for this item.","OK"));

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
                        DisplayAlert("Cancel", "Updates is being cancelled (may take a bit to clean up network communication properly)", "OK");
                        // Just avoid the bar to hang somewhere in between
                        bar.ProgressTo(0, 250, Easing.Linear);
                    });
                IsBusy = false;
                IsGettingData = false;
                return;
            }

            this.IsGettingData = true;
            IsBusy = true;

            try
            {
                // Create Callback and start Work
                await DataManager.startUpdate(new SimpleFinishedCallBack(this, list, bar));
            }
            catch (AuthenticationManager.NotAuthorizedException)
            {
                // handle Authorization Issue
                return;
            }
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

                label.Text = "Starting";

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
                    Thread.Sleep(5000);
                    bool auth = await L2PAPIClient.AuthenticationManager.CheckAuthenticationProgressAsync();
                    if (auth)
                    {
                        label.Text = "done";
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
