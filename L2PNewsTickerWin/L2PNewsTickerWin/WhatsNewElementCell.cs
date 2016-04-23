using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

using L2PAPIClientPortable.DataModel;

namespace L2PNewsTickerWin
{
    public class WhatsNewElementCell : ViewCell
    {
        private Label TitleLabel;
        private Label SecondaryLabel;
        private Label TertiaryLabel;
        private Label TopLabel;

        public WhatsNewElementCell()
        {
            TitleLabel = new Label();
            TitleLabel.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
            TitleLabel.TextColor = MainPage.FontColor;
            TitleLabel.FontAttributes = FontAttributes.Bold;

            SecondaryLabel = new Label();
            SecondaryLabel.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            SecondaryLabel.TextColor = MainPage.FontColor;

            TertiaryLabel = new Label();
            TertiaryLabel.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            TertiaryLabel.TextColor = MainPage.FontColor;

            TopLabel = new Label();
            TopLabel.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
            TopLabel.TextColor = MainPage.SecondaryColor;

            Label paddingLabel = new Label();
            paddingLabel.Text = " ";

            StackLayout stack = new StackLayout();
            stack.Orientation = StackOrientation.Vertical;
            stack.Children.Add(TopLabel);
            stack.Children.Add(TitleLabel);
            stack.Children.Add(SecondaryLabel);
            stack.Children.Add(TertiaryLabel);
            stack.Children.Add(paddingLabel);

            View = stack;

        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext == null) return;

            // Depending on type of content, fill Views differently
            if (BindingContext is L2PAnnouncementElement)
            {
                var bc = (L2PAnnouncementElement)BindingContext;
                TitleLabel.Text = (bc.title == null) ? "" : bc.title;
                SecondaryLabel.Text = (bc.body == null) ? "" : HtmlStripper.StripTagsRegexCompiled(bc.body);
                TertiaryLabel.IsVisible = false;
                TopLabel.Text = Localization.Localize("Announcement");
            }
            else if (BindingContext is L2PAssignmentElement)
            {
                var bc = (L2PAssignmentElement)BindingContext;
                TitleLabel.Text = (bc.title == null) ? "" : bc.title;
                SecondaryLabel.Text = (bc.description == null) ? "" : bc.description;
                TertiaryLabel.Text = "Duedate: " + ((bc.dueDate == null) ? "N/A" : bc.dueDate.ToString());
                TopLabel.Text = Localization.Localize("Assignmemt");
            }
            else if (BindingContext is L2PDiscussionItemElement)
            {
                var bc = (L2PDiscussionItemElement)BindingContext;
                TitleLabel.Text = (bc.subject == null) ? "" : bc.subject;
                SecondaryLabel.Text = (bc.body == null) ? "" : HtmlStripper.StripTagsRegexCompiled(bc.body);
                TertiaryLabel.Text = "by: " + ((bc.from == null) ? "" : bc.from);
                TopLabel.Text = Localization.Localize("DiscussionItem");
            }
            else if (BindingContext is L2PEmailElement)
            {
                var bc = (L2PEmailElement)BindingContext;
                TitleLabel.Text = (bc.subject == null) ? "" : bc.subject;
                SecondaryLabel.Text = (bc.body == null) ? "" : HtmlStripper.StripTagsRegexCompiled(bc.body);
                TertiaryLabel.Text = "from: " + ((bc.from == null) ? "" : bc.from);
                TopLabel.Text = "Email";
            }
            else if (BindingContext is L2PHyperlinkElement)
            {
                var bc = (L2PHyperlinkElement)BindingContext;
                TitleLabel.Text = (bc.url == null) ? "" : bc.url;
                SecondaryLabel.Text = (bc.description == null) ? "" : HtmlStripper.StripTagsRegexCompiled(bc.description);
                TertiaryLabel.Text = "note: " + ((bc.notes == null) ? "" : HtmlStripper.StripTagsRegexCompiled(bc.notes));
                TopLabel.Text = "Hyperlink";
            }
            else if (BindingContext is L2PLiteratureElementDataType)
            {
                var bc = (L2PLiteratureElementDataType)BindingContext;
                TitleLabel.Text = (bc.title == null) ? "" : bc.title;
                SecondaryLabel.Text = (bc.author == null) ? "" : bc.authors;
                //TertiaryLabel.Text = "note: " + HtmlStripper.StripTagsRegexCompiled(bc.notes);
                TertiaryLabel.IsVisible = false;
                TopLabel.Text = Localization.Localize("Literature");
            }
            else if (BindingContext is L2PLearningMaterialElement)
            {
                var bc = (L2PLearningMaterialElement)BindingContext;
                TitleLabel.Text = (bc.name == null) ? "" : bc.name;
                if (bc.isDirectory)
                    SecondaryLabel.Text = Localization.Localize("Folder");
                else
                    SecondaryLabel.Text = Localization.Localize("File");
                //TertiaryLabel.Text = "note: " + HtmlStripper.StripTagsRegexCompiled(bc.notes);
                TertiaryLabel.IsVisible = false;
                TopLabel.Text = Localization.Localize("LearningMaterial")+"/"+ Localization.Localize("SharedDocument");
            }
            else if (BindingContext is L2PMediaLibraryElement)
            {
                var bc = (L2PMediaLibraryElement)BindingContext;
                TitleLabel.Text = (bc.name == null) ? "" : bc.name;
                /*if (bc.isDirectory)
                    SecondaryLabel.Text = "Folder";
                else
                    SecondaryLabel.Text = "File";*/
                SecondaryLabel.IsVisible = false;
                //TertiaryLabel.Text = "note: " + HtmlStripper.StripTagsRegexCompiled(bc.notes);
                TertiaryLabel.IsVisible = false;
                TopLabel.Text = Localization.Localize("MediaLibrary");
            }
            else if (BindingContext is L2PWikiElement)
            {
                var bc = (L2PWikiElement)BindingContext;
                TitleLabel.Text = (bc.title == null) ? "" : bc.title;
                SecondaryLabel.IsVisible = false;
                //TertiaryLabel.Text = "note: " + HtmlStripper.StripTagsRegexCompiled(bc.notes);
                TertiaryLabel.IsVisible = false;
                TopLabel.Text = "Wiki";
            }
            else
            {
                // Fallback
                TitleLabel.Text = "Unknown";
            }
        }
    }
}
