using System;
using Xamarin.Forms;
using Android.Content;
using Android.App;
using Plugin.Connectivity;
using System.Linq;


namespace WiFiConnect
{
    public partial class MainPage : ContentPage
    {
        bool passwwordViewPermitted = false;
        bool dpPasswwordViewPermitted = false;
        string networkKeyValue = "";
        string dpNetworkKeyValue = "";
        public static MainPage mainPage;
        BasicAlgorthm ba;
        

        public MainPage()
        {
            InitializeComponent();
            mainPage = this;
            SizeChanged += OnPageSizeChanged;
            ba = new BasicAlgorthm();
            DevicePortalSettings.IsVisible = false;
            object name = "";
            if (!App.Current.Properties.TryGetValue("devicePortalPassword", out name))
            {//Если объекта в хранилище нет
                App.Current.Properties["devicePortalPassword"] = "admin";
            }

            CommonStruct.devicePortalPassword = (App.Current.Properties["devicePortalPassword"].ToString());
            
            DevicePortalSettings.PropertyChanging += DevicePortalSettings_PropertyChanging;
            //var currentuserID = (App.Current.Properties["devicePortalPassword"].ToString());
            //App.Current.Properties.TryGetValue("devicePortalPassword", out name);
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
                //PasswordField1.Orientation = StackOrientation.Vertical;
                //PasswordField2.HorizontalOptions = LayoutOptions.StartAndExpand;

                if ((Height > 1000) && (Width > 700))
                {
                    //PasswordField1.Orientation = StackOrientation.Vertical;
                    //PasswordField2.HorizontalOptions = LayoutOptions.StartAndExpand;
                    //stringPortret.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
                    stringLandscape.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    //string2.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    //string3.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    //string4.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    //string5.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
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

                //PasswordField1.Orientation = StackOrientation.Horizontal;
                //PasswordField2.HorizontalOptions = LayoutOptions.EndAndExpand;

                if ((Height < 1000) || (Width < 700))
                {
                    //stringPortret.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
                    
                    stringLandscape.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    //string2.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    //string3.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    //string4.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    //string5.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
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

        public async void ConnectButton_ClickAsync(object sender, EventArgs e)
        {
            string ssid = enteredSsid.Text;
            //ssid = "RLDA_NET";
            networkKeyValue = networkKey.Text;
            //networkKeyValue = "pr0tectnetw0rk13";
            var wifi = Plugin.Connectivity.Abstractions.ConnectionType.WiFi;
            var connectionTypes = CrossConnectivity.Current.ConnectionTypes;
            NotifyUser("", NotifyType.Empty);

            if ((ssid == null) || (ssid == ""))
            {
                NotifyUser("Enter network SSID (Name) in field above", NotifyType.ErrorMessage);
                return;
            }
            else if ((networkKeyValue == null) || (networkKeyValue == ""))
            {
                NotifyUser("Enter network security key in field above", NotifyType.ErrorMessage);
                return;
            }
            else if (!connectionTypes.Contains(wifi))
            {
                NotifyUser("You are not connected to WiFi", NotifyType.ErrorMessage);
                return;
            }
            else if (CommonStruct.devicePortalPassword == null)
            {
                NotifyUser("Enter Device Portal password ", NotifyType.ErrorMessage);
                ButtonSettings_Click(null, null);
                return;
            }
            else
            {
                NotifyUser("Waiting for connection...", NotifyType.StatusMessage);
                //string result = await ba.ConnectRobotToWiFiAsync(ssid, networkKeyValue, "");

                string result = await ba.IteratorAsync(ssid, networkKeyValue, "", ba.ConnectRobotToWiFiAsync);

                if (result == "WrongNet")
                {
                    NotifyUser("Check if your gadget is connected to BotEyes Access Point AJ_...", NotifyType.ErrorMessage);
                    return;
                }

                if (result == "Internal server error")
                {
                    NotifyUser("Check SSID and Key in fields above.", NotifyType.ErrorMessage);
                    return;
                }
                if (result == "GuidFail")
                {
                    NotifyUser("Check BotEyes Device Portal Password (Press button 'Settings' below).", NotifyType.ErrorMessage);
                    return;
                }

                if (result == "Unauthorized")
                {
                    NotifyUser("Password for Device Portal is wrong.", NotifyType.ErrorMessage);
                    return;
                }
                else if (result != "OK")
                {
                    NotifyUser("Something is wrong. Check if your gadget is connected to BotEyes access point 'AJ_...'", NotifyType.ErrorMessage);
                    return;
                }
                else
                {
                    NotifyUser("BotEyes is connected to WiFi successfully.", NotifyType.StatusMessage);
                }
            }
        }

       

        void buttonGo_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("https://boteyes.com/");
            Device.BeginInvokeOnMainThread(() =>
            {
                Device.OpenUri(uri);
            });
        }

        private void manualButton_Clicked(object sender, EventArgs e)
        {
            Uri uri = new Uri("https://boteyes.com/HelpEng.html");
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
                case NotifyType.Empty:
                    StatusBorder.BackgroundColor = Color.White;
                    break;
            }
            StatusBlock.Text = "   " + strMessage;
            BatchBegin();
            ForceLayout();
        }

        
        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            devicePortalPassword.Text = CommonStruct.devicePortalPassword;

            NotifyUser("", NotifyType.Empty);
            DevicePortalSettings.IsVisible = true;

            

        }

        private void DevicePortalSettings_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            ScrollOfPage.ScrollToAsync(3000, 6000, true);
        }

        

        private void PasswordEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void LoginEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage,
            Empty
        };

        private async void Save_ClickedAsync(object sender, EventArgs e)
        {
            CommonStruct.devicePortalPassword = devicePortalPassword.Text;
            App.Current.Properties["devicePortalPassword"] = CommonStruct.devicePortalPassword;
            await App.Current.SavePropertiesAsync();
            
            DevicePortalSettings.IsVisible = false;
        }

        private void Discard_Clicked(object sender, EventArgs e)
        {
            DevicePortalSettings.IsVisible = false;
        }

        private void DpPasswordViewSwitcher_Toggled(object sender, ToggledEventArgs e)
        {
            //devicePortalPassword
            try
            {
                dpPasswwordViewPermitted = DpPasswordViewSwitcher.IsToggled;

                if (dpPasswwordViewPermitted == true)
                {
                    DpPasswordSwitchLabel.Text = "To hide entered key turn the switch OFF";
                    dpNetworkKeyValue = devicePortalPassword.Text;
                    devicePortalPassword.IsPassword = false;
                }
                else
                {
                    DpPasswordSwitchLabel.Text = "Turn switch ON to view entered key";
                    devicePortalPassword.IsPassword = true;
                }
            }
            catch(Exception e1)
            {
            }
        }
    }
}