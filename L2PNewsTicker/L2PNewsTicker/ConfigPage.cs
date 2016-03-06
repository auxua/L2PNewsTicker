using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace L2PNewsTicker
{
    public class ConfigPage : ContentPage
    {

        int days;
        int hours;
        int minutes;

        Label StatusLabel;

        public ConfigPage()
        {
            StackLayout StepperStack = new StackLayout();
            StepperStack.Orientation = StackOrientation.Vertical;
            StepperStack.Padding = new Thickness(10, 10);

            Label TopLabel = new Label();
            TopLabel.Text = Localization.Localize("TimeSpanSince");

            StatusLabel = new Label();
            StatusLabel.Text = "";


            Button SaveButton = new Button();
            SaveButton.Text = Localization.Localize("SaveChanges");
            SaveButton.Clicked += SaveButton_Clicked;

            // Get Time from current Config
            object time;
            int since;
            if (Application.Current.Properties.TryGetValue("since", out time))
            {
                since = (int)time;
            }
            else
            {
                since = 60 * 24;
            }
            // Convert for Steppers
            days = since / (60 * 24);
            //int hours = (since % 60) % 24;
            hours = (since - (days * 24*60)) / 60;
            minutes = since - (hours * 60) - (days*60*24);

            StackLayout DayStack = new StackLayout();
            DayStack.Orientation = StackOrientation.Horizontal;
            Label DayLabel = new Label();
            DayLabel.Text = "Days";
            DayLabel.VerticalOptions = LayoutOptions.Center;
            Label DayCountLabel = new Label();
            DayCountLabel.Text = days.ToString();
            DayCountLabel.VerticalOptions = LayoutOptions.Center;
            Stepper DayStepper = new Stepper();
            DayStepper.Minimum = 0;
            DayStepper.Value = days;
            DayStepper.Maximum = 120;
            DayStepper.ValueChanged += ((x,y) => {
                DayCountLabel.Text = DayStepper.Value.ToString();
                days = (int)DayStepper.Value;
                });
            DayStack.Children.Add(DayLabel);
            DayStack.Children.Add(DayCountLabel);
            DayStack.Children.Add(DayStepper);
            DayStack.HorizontalOptions = LayoutOptions.End;
            //DayStack.VerticalOptions = LayoutOptions.Center;

            StackLayout HourStack = new StackLayout();
            HourStack.Orientation = StackOrientation.Horizontal;
            Label HourLabel = new Label();
            HourLabel.Text = "Hours";
            HourLabel.VerticalOptions = LayoutOptions.Center;
            Label HourCountLabel = new Label();
            HourCountLabel.Text = hours.ToString();
            HourCountLabel.VerticalOptions = LayoutOptions.Center;
            Stepper HourStepper = new Stepper();
            HourStepper.Minimum = 0;
            HourStepper.Maximum = 23;
            HourStepper.Value = hours;
            HourStepper.ValueChanged += ((x, y) =>
            {
                HourCountLabel.Text = HourStepper.Value.ToString();
                hours = (int)HourStepper.Value;
            });
            HourStack.Children.Add(HourLabel);
            HourStack.Children.Add(HourCountLabel);
            HourStack.Children.Add(HourStepper);
            HourStack.HorizontalOptions = LayoutOptions.End;


            StackLayout MinuteStack = new StackLayout();
            MinuteStack.Orientation = StackOrientation.Horizontal;
            Label MinuteLabel = new Label();
            MinuteLabel.Text = "Hours";
            MinuteLabel.VerticalOptions = LayoutOptions.Center;
            Label MinuteCountLabel = new Label();
            MinuteCountLabel.Text = minutes.ToString();
            MinuteCountLabel.VerticalOptions = LayoutOptions.Center;
            Stepper MinuteStepper = new Stepper();
            MinuteStepper.Minimum = 0;
            MinuteStepper.Maximum = 59;
            MinuteStepper.Value = minutes;
            MinuteStepper.ValueChanged += ((x, y) => {
                MinuteCountLabel.Text = MinuteStepper.Value.ToString();
                minutes = (int)MinuteStepper.Value;

                });
            MinuteStack.Children.Add(MinuteLabel);
            MinuteStack.Children.Add(MinuteCountLabel);
            MinuteStack.Children.Add(MinuteStepper);
            MinuteStack.HorizontalOptions = LayoutOptions.End;

            StepperStack.Children.Add(TopLabel);
            StepperStack.Children.Add(DayStack);
            StepperStack.Children.Add(HourStack);
            StepperStack.Children.Add(MinuteStack);
            StepperStack.Children.Add(SaveButton);
            StepperStack.Children.Add(StatusLabel);

            Title = Localization.Localize("Config");

            Content = StepperStack;

        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            int time = minutes + (60 * hours) + (24 * 60 * days);
            // Prevent user from very small timespans
            if (time < 15)
            {
                Device.BeginInvokeOnMainThread(() => DisplayAlert("Error", Localization.Localize("MinTime"), "OK"));
                return;
            }
            Application.Current.Properties["since"] = time;
            Device.BeginInvokeOnMainThread(() => StatusLabel.Text = "✓");
        }
    }
}
