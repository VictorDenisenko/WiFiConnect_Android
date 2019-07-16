using Android;
using Android.Content;
using Android.Net.Wifi;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using WiFiConnect.Droid;
using Xamarin.Forms;


[assembly: Dependency(typeof(WiFiAccess))]
namespace WiFiConnect.Droid
{
    delegate string FunctionTheSameAddress();

    public class WiFiAccess : IWiFiInterface 
    {
        WifiManager wifiManager = null;
        MainPage mp;
        int robotNetworkId = -1;
        int gadgetNetworkId = -1;
        BasicAlgorthm ba;
        ScanResult LastConnectedNetwork = null;
        string networkKey = "";
        string SoftApSsid = "";
        MainActivity owner;

        public static IAccessPointHelper AccessPointHelper { get; set; }
        private ObservableCollection<AccessPoint> _AvailableAccessPoints = new ObservableCollection<AccessPoint>();

        public WiFiAccess()
        {
            mp = MainPage.mainPage;
            ba = new BasicAlgorthm();
            owner = MainActivity.owner;
        }

        public void ConnectToWifi(string _networkKey)
        {





            networkKey = _networkKey;
            var ver = Android.OS.Build.VERSION.Release;
            if ((ver.StartsWith("8")) || (ver.StartsWith("7")) || (ver.StartsWith("6")))
            {
                AccessPointHelper = new WifiHelper();
                AccessPointHelper.AccessPointsEnumeratedEvent += AccessPointHelper_AccessPointsEnumeratedEvent;
                FindAccessPoints(null, null);

            }
            else
            {

                AccessPointHelper = new WifiHelper();
                AccessPointHelper.AccessPointsEnumeratedEvent += AccessPointHelper_AccessPointsEnumeratedEvent;
                FindAccessPoints(null, null);
            }
        }

        private void AccessPointHelper_AccessPointsEnumeratedEvent(string obj)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                //mp.ConnectingAlgorithmStart();
                ConnectToWifi_Old(networkKey);
            });
        }

        public void FindAccessPoints(object sender, System.EventArgs e)
        {
            AccessPointHelper.FindAccessPoints(_AvailableAccessPoints);
        }

        public async void  ConnectToWifi_Old(string _networkKey)
        {
            ScanResult SoftApNetwork = null;
            networkKey = _networkKey;
            try
            {
                wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

                if (!wifiManager.IsWifiEnabled)
                {
                    wifiManager.SetWifiEnabled(true);
                }
                var myWifiLock = wifiManager.CreateWifiLock("myWifiLock");
                myWifiLock.Acquire();

                var scanAvailable = wifiManager.IsScanAlwaysAvailable;
                var isEnabled = wifiManager.IsWifiEnabled;

                var ResultCollection = wifiManager.ScanResults;//Это список всех сетей, к которым можно подключиться

                for (int i = 0; i < ResultCollection.Count; i++)
                {
                    if (ResultCollection[i].Bssid == wifiManager.ConnectionInfo.BSSID)
                    {
                        LastConnectedNetwork = ResultCollection[i];//Запоминаем объект с настройками сети, к которой подключен гаджет
                        gadgetNetworkId = wifiManager.ConnectionInfo.NetworkId;
                        CommonStruct.lastConnectedSsid = wifiManager.ConnectionInfo.SSID;
                    }
                    if (ResultCollection[i].Ssid.StartsWith("AJ_"))
                    {
                        SoftApNetwork = ResultCollection[i];//Запоминаем объект с настройками сети робота
                        SoftApSsid = SoftApNetwork.Ssid;
                    }
                }

                if ((wifiManager.ConnectionInfo.BSSID == null) || (wifiManager.ConnectionInfo.NetworkId == -1))
                {
                    mp.NotifyUser("Your gadget has not been connected to WiFi access point", MainPage.NotifyType.ErrorMessage);
                }
                else if (LastConnectedNetwork != null)
                {
                    //string resultConnect = await ba.IteratorAsync(LastConnectedNetwork.Ssid, networkKey, "", ba.ConnectRobotToWiFiAsync);

                    

                    string x = await ba.ConnectRobotToWiFiAsync("RLDA_NET", "pr0tectnetw0rk13", "");

                }
            }
            catch (Exception ex)
            {
                mp.NotifyUser(ex.Message, MainPage.NotifyType.ErrorMessage);
                wifiManager.Disconnect();
                wifiManager.DisableNetwork(robotNetworkId);//DisableNetwork предотвращает автоматичекое соединение
            }
        }

    }
}
