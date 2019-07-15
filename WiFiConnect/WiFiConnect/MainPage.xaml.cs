using Plugin.Settings;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace WiFiConnect
{
    public partial class MainPage : ContentPage
    {
        bool passwwordViewPermitted = false;
        string networkKeyValue = "";
        public static MainPage mainPage;

        public MainPage()
        {
            InitializeComponent();
            mainPage = this;
            SizeChanged += OnPageSizeChanged;

            CrossSettings.Current.AddOrUpdateValue("name", "Tom");
            //CrossSettings.Current.AddOrUpdateValue<string>("name", "Tom");
        }

        public void Dispose()
        {
            SizeChanged -= OnPageSizeChanged;
        }

        void OnPageSizeChanged(object sender, EventArgs args)
        {
            //double maxWidth = 800;
            double maxHeight = 1256;
            double factor = Height / maxHeight;
            double logoSize = 225 * 0.6;
            LogoPortret.HeightRequest = factor * logoSize;
            LogoLandscape.HeightRequest = factor * logoSize;

            double buttonHeight = StartButton.Height;

            if (Height > Width)
            {//Portret
                LogoPortret.IsVisible = true;
                LogoLandscape.IsVisible = false;
                stringPortret.IsVisible = true;
                stringLandscape.IsVisible = false;
                PasswordField1.Orientation = StackOrientation.Vertical;
                PasswordField2.HorizontalOptions = LayoutOptions.StartAndExpand;



                if ((Height > 1000) && (Width > 700))
                {
                    PasswordField1.Orientation = StackOrientation.Vertical;
                    PasswordField2.HorizontalOptions = LayoutOptions.StartAndExpand;
                    //stringPortret.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
                    stringLandscape.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    string2.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    string3.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    string4.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    string5.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    PasswordSwitchLabel.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    StatusBlock.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));

                    GoToSiteButton.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    StartButton.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    UserManualButton.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));

                    StartButton.HeightRequest = buttonHeight * 1.5;
                    GoToSiteButton.HeightRequest = buttonHeight * 1.5;
                    UserManualButton.HeightRequest = buttonHeight * 1.5;
                }
            }
            else 
            {
                LogoPortret.IsVisible = false;
                LogoLandscape.IsVisible = true;
                stringPortret.IsVisible = false;
                stringLandscape.IsVisible = true;

                PasswordField1.Orientation = StackOrientation.Horizontal;
                PasswordField2.HorizontalOptions = LayoutOptions.EndAndExpand;

                if ((Height < 1000) || (Width < 700))
                {
                    //stringPortret.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
                    
                    stringLandscape.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    string2.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    string3.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    string4.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    string5.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    PasswordSwitchLabel.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    StatusBlock.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));

                    GoToSiteButton.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    StartButton.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    UserManualButton.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));

                    //StartButton.HeightRequest = buttonHeight * 1.5;
                    //GoToSiteButton.HeightRequest = buttonHeight * 1.5;
                    //UserManualButton.HeightRequest = buttonHeight * 1.5;
                }
            }
        }

        void ConnectButton_Click(object sender, EventArgs e)
        {
            networkKeyValue = networkKey.Text;
            //networkKeyValue = "pr0tectnetw0rk13";

            if ((networkKeyValue == null) || (networkKeyValue == ""))
            {
                NotifyUser("Enter network security key in field above", NotifyType.ErrorMessage);
            }
            else
            {
                NotifyUser("Waiting for connection...", NotifyType.StatusMessage);
                ConnectingAlgorithmStart();
            }
        }

        public void ConnectingAlgorithmStart()
        {
            var connector = DependencyService.Get<IWiFiInterface>();
            connector.ConnectToWifi(networkKeyValue);
        }

        void buttonGo_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("https://boteyes.com/DefaultEng.aspx");
            Device.BeginInvokeOnMainThread(() =>
            {
                Device.OpenUri(uri);
            });
        }

        private void manualButton_Clicked(object sender, EventArgs e)
        {
            Uri uri = new Uri("https://boteyes.com/HelpEng.aspx");
            Device.BeginInvokeOnMainThread(() =>
            {
                Device.OpenUri(uri);
            });
        }

       private void PasswordViewSwitcher_Toggled(object sender, ToggledEventArgs e)
        {
            passwwordViewPermitted = PasswordViewSwitcher.IsToggled;
            
            if (passwwordViewPermitted == true)
            {
                PasswordSwitchLabel.Text = "To hide entered key turn the switch OFF";
                networkKeyValue = networkKey.Text;
                networkKey.IsPassword = false;
            }
            else
            {
                PasswordSwitchLabel.Text = "Turn switch ON to view entered key";
                networkKey.IsPassword = true;
            }
        }

        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.BackgroundColor = Color.Green;
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.BackgroundColor = Color.Red;
                    break;
            }
            StatusBlock.Text = "   " + strMessage;
            BatchBegin();
            ForceLayout();
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };
    }
}