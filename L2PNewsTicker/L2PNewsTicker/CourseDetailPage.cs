using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace L2PNewsTicker
{
    public class CourseDetailPage : ContentPage
    {
        public CourseDetailPage(IEnumerable<object> data)
        {
            Title = "Course Detail";

            ListView list = new ListView();
            list.ItemTemplate = new DataTemplate(typeof(WhatsNewElementCell));
            list.HasUnevenRows = true;

            ContentView view = new ContentView();
            view.Content = list;

            this.Padding = new Thickness(10, 10);

            Content = view;

            BackgroundColor = MainPage.Background;

            //list.ItemsSource = data.announcements;
            list.ItemsSource = data;
        }
    }
}
