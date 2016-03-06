﻿using System;
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
            BackgroundColor = MainPage.Background;

            StackLayout StepperStack = new StackLayout();
            StepperStack.Orientation = StackOrientation.Vertical;
            StepperStack.Padding = new Thickness(10, 10);

            Label TopLabel = new Label();
            TopLabel.Text = Localization.Localize("TimeSpanSince");
            TopLabel.TextColor = MainPage.FontColor;

            StatusLabel = new Label();
            StatusLabel.Text = " ";
            StatusLabel.TextColor = MainPage.FontColor;
            StatusLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            StatusLabel.VerticalOptions = LayoutOptions.Center;

            Button SaveButton = new Button();
            SaveButton.Text = Localization.Localize("SaveChanges");
            SaveButton.Clicked += SaveButton_Clicked;
            SaveButton.TextColor = MainPage.FontColor;
            SaveButton.HorizontalOptions = LayoutOptions.CenterAndExpand;

            StackLayout StatusStack = new StackLayout();
            StatusStack.Orientation = StackOrientation.Horizontal;
            StatusStack.Children.Add(SaveButton);
            //StatusStack.Children.Add(StatusLabel);

            Button FAQButton = new Button();
            FAQButton.Text = "FAQs (Online)";
            FAQButton.TextColor = MainPage.FontColor;
            FAQButton.Clicked += ((X, y) => Device.OpenUri(new Uri("https://apps.auxua.eu")));
            FAQButton.HorizontalOptions = LayoutOptions.Center;

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
            DayLabel.Text = Localization.Localize("Days");
            DayLabel.VerticalOptions = LayoutOptions.Center;
            DayLabel.TextColor = MainPage.FontColor;
            Label DayCountLabel = new Label();
            DayCountLabel.Text = days.ToString();
            DayCountLabel.VerticalOptions = LayoutOptions.Center;
            DayCountLabel.TextColor = MainPage.FontColor;
            Stepper DayStepper = new Stepper();
            DayStepper.Minimum = 0;
            DayStepper.Value = days;
            DayStepper.Maximum = 120;
            DayStepper.ValueChanged += ((x,y) => {
                DayCountLabel.Text = DayStepper.Value.ToString();
                days = (int)DayStepper.Value;
                });
			if (Device.OS == TargetPlatform.iOS) DayStepper.BackgroundColor = MainPage.FontColor;
            DayStack.Children.Add(DayLabel);
            DayStack.Children.Add(DayCountLabel);
            DayStack.Children.Add(DayStepper);
            DayStack.HorizontalOptions = LayoutOptions.End;
            //DayStack.VerticalOptions = LayoutOptions.Center;

            StackLayout HourStack = new StackLayout();
            HourStack.Orientation = StackOrientation.Horizontal;
            Label HourLabel = new Label();
            HourLabel.Text = Localization.Localize("Hours");
            HourLabel.VerticalOptions = LayoutOptions.Center;
            HourLabel.TextColor = MainPage.FontColor;
            Label HourCountLabel = new Label();
            HourCountLabel.Text = hours.ToString();
            HourCountLabel.VerticalOptions = LayoutOptions.Center;
            HourCountLabel.TextColor = MainPage.FontColor;
            Stepper HourStepper = new Stepper();
            HourStepper.Minimum = 0;
            HourStepper.Maximum = 23;
            HourStepper.Value = hours;
            HourStepper.ValueChanged += ((x, y) =>
            {
                HourCountLabel.Text = HourStepper.Value.ToString();
                hours = (int)HourStepper.Value;
            });
			if (Device.OS == TargetPlatform.iOS) HourStepper.BackgroundColor = MainPage.FontColor;
            HourStack.Children.Add(HourLabel);
            HourStack.Children.Add(HourCountLabel);
            HourStack.Children.Add(HourStepper);
            HourStack.HorizontalOptions = LayoutOptions.End;


            StackLayout MinuteStack = new StackLayout();
            MinuteStack.Orientation = StackOrientation.Horizontal;
            Label MinuteLabel = new Label();
            MinuteLabel.Text = Localization.Localize("Minutes");
            MinuteLabel.VerticalOptions = LayoutOptions.Center;
            MinuteLabel.TextColor = MainPage.FontColor;
            Label MinuteCountLabel = new Label();
            MinuteCountLabel.Text = minutes.ToString();
            MinuteCountLabel.VerticalOptions = LayoutOptions.Center;
            MinuteCountLabel.TextColor = MainPage.FontColor;
            Stepper MinuteStepper = new Stepper();
            MinuteStepper.Minimum = 0;
            MinuteStepper.Maximum = 59;
            MinuteStepper.Value = minutes;
            MinuteStepper.ValueChanged += ((x, y) => {
                MinuteCountLabel.Text = MinuteStepper.Value.ToString();
                minutes = (int)MinuteStepper.Value;

                });
			if (Device.OS == TargetPlatform.iOS) MinuteStepper.BackgroundColor = MainPage.FontColor;
            MinuteStack.Children.Add(MinuteLabel);
            MinuteStack.Children.Add(MinuteCountLabel);
            MinuteStack.Children.Add(MinuteStepper);
            MinuteStack.HorizontalOptions = LayoutOptions.End;

            StepperStack.Children.Add(TopLabel);
            StepperStack.Children.Add(DayStack);
            StepperStack.Children.Add(HourStack);
            StepperStack.Children.Add(MinuteStack);
            //StepperStack.Children.Add(SaveButton);
            //StepperStack.Children.Add(StatusLabel);
            StepperStack.Children.Add(StatusStack);
            StepperStack.Children.Add(FAQButton);

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
            //Device.BeginInvokeOnMainThread(() => StatusLabel.Text = "✓");
        }
    }
}
