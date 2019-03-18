using Android.Content;
using Android.Net.Wifi;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace WiFiConnect.Droid
{
    public class WifiHelper : IAccessPointHelper
    {
        public event Action<string> AccessPointsEnumeratedEvent;
        private SemaphoreSlim _SocketLock = new SemaphoreSlim(1, 1);
        private WifiReceiver _WifiReceiver = null;
        MainPage mp;

        public WifiHelper()
        {
            mp = MainPage.mainPage;
        }

        class WifiReceiver : BroadcastReceiver
        {
            ObservableCollection<AccessPoint> _AvailableAccessPoints;
            WifiHelper _Helper;
            MainPage mp;

            public WifiReceiver(WifiHelper helper, ObservableCollection<AccessPoint> availableAccessPoints)
            {
                _AvailableAccessPoints = availableAccessPoints;
                _Helper = helper;
                mp = MainPage.mainPage;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    Android.App.Application.Context.UnregisterReceiver(_Helper._WifiReceiver);
                    var wifiManager = Android.App.Application.Context.GetSystemService(Context.WifiService) as WifiManager;
                    wifiManager.SetWifiEnabled(true);
                    var scanWifiNetworks = wifiManager.ScanResults;
                    var count = scanWifiNetworks.Count;

                    if (count == 0)
                    {
                        mp.NotifyUser("Please, press the button 'Press here to connect' again.", MainPage.NotifyType.StatusMessage);
                    }

                    scanWifiNetworks.Select(x => x.Ssid).
                                     Distinct().
                                     OrderBy(ssid => ssid).ToList().
                                     ForEach(ssid =>
                                     {
                                         _AvailableAccessPoints.Add(new AccessPoint() { Ssid = ssid });
                                     });

                    if (_Helper.AccessPointsEnumeratedEvent != null)
                    {
                        _Helper.AccessPointsEnumeratedEvent("Enumerated");
                    }
                }
                catch(Exception ex)
                {
                    mp.NotifyUser(ex.Message, MainPage.NotifyType.ErrorMessage);
                }
            }
        }

        public void FindAccessPoints(ObservableCollection<AccessPoint> availableAccessPoints)
        {
            try
            {
                availableAccessPoints.Clear();
                var wifiManager = Android.App.Application.Context.GetSystemService(Context.WifiService) as WifiManager;
                if (wifiManager.IsWifiEnabled)
                {
                    if (_WifiReceiver == null)
                    {
                        _WifiReceiver = new WifiReceiver(this, availableAccessPoints);
                    }
                    Android.App.Application.Context.RegisterReceiver(_WifiReceiver, new IntentFilter(WifiManager.ScanResultsAvailableAction));
                    wifiManager.StartScan();
                }
                else if (AccessPointsEnumeratedEvent != null)
                {
                    AccessPointsEnumeratedEvent("Enumerated");
                }
            }
            catch(Exception ex)
            {
                mp.NotifyUser(ex.Message, MainPage.NotifyType.ErrorMessage);
            }
        }
    }
}
