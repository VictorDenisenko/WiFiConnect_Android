using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using WiFiConnect;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.Networking.Connectivity;

namespace WiFiConnect.UWP
{
    public class WifiHelper
    {
        public WiFiAdapter firstAdapter;
        public static MainPage Current;
        public PasswordCredential credential;
        public ConnectionProfile connectedProfile = null;

        public WifiHelper()
        {

        }

        async void LookingForAdapter(NavigationEventArgs e)
        {
            ResultCollection = new ObservableCollection<WiFiAvailableNetwork>();
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                //NotifyUser("Access denied", NotifyType.ErrorMessage);
            }
            else
            {
                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (result.Count >= 1)
                {
                    firstAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);//Это сетевй адаптер - железо, стоящее на этом компе 
                }
                else
                {
                   // NotifyUser("No WiFi Adapters detected on this machine.", NotifyType.ErrorMessage);
                }
            }
        }

        public ObservableCollection<WiFiAvailableNetwork> ResultCollection
        {
            get;
            private set;
        }
    }

    
}
