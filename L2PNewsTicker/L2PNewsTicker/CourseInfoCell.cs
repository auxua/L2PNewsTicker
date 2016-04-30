using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace L2PNewsTicker
{
    public class CourseInfoCell : ViewCell
    {
        public CourseInfoCell()
        {
            StackLayout stack = new StackLayout();
            Label cidLabel = new Label();
            Label courseName = new Label();
            Label newInfo = new Label();
            Label paddingLabel = new Label();

            stack.Orientation = StackOrientation.Vertical;
            cidLabel.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
            cidLabel.TextColor = MainPage.SecondaryColor;
            courseName.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
            courseName.TextColor = MainPage.FontColor;
            newInfo.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            newInfo.TextColor = MainPage.FontColor;
            paddingLabel.Text = " ";

            stack.Children.Add(cidLabel);
            stack.Children.Add(courseName);
            stack.Children.Add(newInfo);
            stack.Children.Add(paddingLabel);

            cidLabel.SetBinding(Label.TextProperty, "cid");
            courseName.SetBinding(Label.TextProperty, "Text");
            newInfo.SetBinding(Label.TextProperty, "Detail");

            View = stack;
        }
    }
}
