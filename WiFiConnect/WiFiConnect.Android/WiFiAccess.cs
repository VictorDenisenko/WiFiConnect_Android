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

    public class WiFiAccess : IWiFiInterface, MainActivity.IPermissionCallback
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
        readonly static int ACCESS_FINE_LOCATION = 0;
        string permissions = "Yes";

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
                permissions = "No";
                owner.AskForPermission(Manifest.Permission.AccessFineLocation, 0, this);
            }
            else{
                permissions = "Yes";
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
                var isEnabled =  wifiManager.IsWifiEnabled;

                var ResultCollection = wifiManager.ScanResults;//Это список всех сетей, к которым можно подключиться

                for (int i = 0; i < ResultCollection.Count; i++)
                {
                    if (ResultCollection[i].Bssid == wifiManager.ConnectionInfo.BSSID)
                    {
                        LastConnectedNetwork = ResultCollection[i];//Запоминаем объект с настройками сети, к которой подключен гаджет
                        gadgetNetworkId = wifiManager.ConnectionInfo.NetworkId;
                        CommonStruct.lastConnectedSsid = wifiManager.ConnectionInfo.SSID;
                    }
                    if (ResultCollection[i].Ssid.StartsWith("AJ_SoftAPSsid_"))
                    {
                        SoftApNetwork = ResultCollection[i];//Запоминаем объект с настройками сети робота
                        SoftApSsid = SoftApNetwork.Ssid;

                    }
                }
                
                if ((wifiManager.ConnectionInfo.BSSID == null)||(wifiManager.ConnectionInfo.NetworkId == -1))
                {
                    mp.NotifyUser("Your gadget has not been connected to WiFi access point", MainPage.NotifyType.ErrorMessage);
                }
                else if (LastConnectedNetwork != null)
                {
                    if (LastConnectedNetwork.Ssid.StartsWith("AJ_SoftAPSsid_"))
                    {
                        mp.NotifyUser("WiFi access point " + LastConnectedNetwork.Ssid + " cannot be used for this purpose. Choose another.", MainPage.NotifyType.ErrorMessage);
                    }
                    else if (SoftApNetwork == null)
                    {
                        mp.NotifyUser("Check if your Robot is ON and 'WiFi Reset' button on the robot was pressed. Check if your gadget has found the robot access point AJ_SoftAPSsid_... (See the list of available networks)", MainPage.NotifyType.ErrorMessage);
                    }
                    else if ((SoftApNetwork != null) && (wifiManager.ConnectionInfo.BSSID != null))
                    {//Проверяем, что все в порядке: было подключение гаджета к сети, робот включен и вещает SoftAP
                        wifiManager.Disconnect();
                        wifiManager.DisableNetwork(gadgetNetworkId);//DisableNetwork предотвращает автоматичекое соединение
                        var configuredNetworks = wifiManager.ConfiguredNetworks;//Получаем список сетей, для которых в гаджете есть профайлы 
                        bool success = false;

                        for (int k = 0; k < wifiManager.ConfiguredNetworks.Count; k++)
                        {//Ищем уже существующий профайл сети робота. Если этого не делать, то в списке появляется много одинаковых сетей 
                            if (configuredNetworks[k].Ssid == "\"" + SoftApNetwork.Ssid +"\"")//В профайлах такой формат, а в доступных сетях - без косой черты
                            {
                                success = wifiManager.EnableNetwork(configuredNetworks[k].NetworkId, true);//Подсоединяемся к SoftAP Raspberry без пароля (если найден готовый профайл)
                                robotNetworkId = configuredNetworks[k].NetworkId;
                                if (success == true) break;
                            }
                        }
                        if (success == false)
                        {//Если просто активировать профайл не удалось, то подсоединяемся к SoftAP Raspberry с паролем
                            string result = IteratorTheSameAddress(ConnectToRaspberry);
                            robotNetworkId = wifiManager.ConnectionInfo.NetworkId;
                            if (result != "OK") { success = false; }
                        }

                        if (success == false)
                        {//Если ни одним из двух способов подключиться не удалось, то отсоединяемся от Распберри и подключаемся к той прежней сети
                            wifiManager.Disconnect();//Отсоединяемся от SoftAP
                            wifiManager.DisableNetwork(robotNetworkId);//DisableNetwork предотвращает автоматичекое соединение
                            string result = IteratorTheSameAddress(ConnectToTheSameNetwork);//Подключаемся к той же сети, к которой был подключен гаджет до того, как подключили Распберри
                            mp.NotifyUser("Check if 'WiFi Reset' button on the robot was pressed. Check if your gadget has found the robot access point AJ_SoftAPSsid_... (See the list of available networks).", MainPage.NotifyType.ErrorMessage);
                        }
                        else
                        {//Если подключились к Распберри, то проверяем наличие связи с Device Portal:
                            //Ping pingSender = new Ping();//работает только с Виндовс
                            //PingReply reply = pingSender.Send("192.168.137.1");//работает только с Виндовс
                            //Порверяем доступность канала до Device Portal:
                            string result = await ba.IteratorAsync(CommonStruct.robotNetworkPassword, CommonStruct.robotNetworkAdminName, "", ba.LookingForInfo);
                            if (result != "OK")
                            {
                                mp.NotifyUser("Something is wrong. May be you need to reboot the robot...", MainPage.NotifyType.ErrorMessage);
                                wifiManager.Disconnect();//Отсоединяемся от SoftAP
                                wifiManager.DisableNetwork(robotNetworkId);//DisableNetwork предотвращает автоматичекое соединение
                                string answer2 = ConnectToTheSameNetwork();//Подключаемся к той же сети, к которой был подключен гаджет до того, как подключили Распберри
                            }
                            else
                            {
                                string resultIpAddress = await ba.IteratorAsync(CommonStruct.robotNetworkPassword, CommonStruct.robotNetworkAdminName, "192.168.137.1:8080", ba.LookingForIPAddress);
                                if (resultIpAddress == "OK")
                                {
                                    mp.NotifyUser("Your robot is already connected to WiFi. IP address is " + CommonStruct.robotIpAddress, MainPage.NotifyType.StatusMessage);
                                    wifiManager.Disconnect();//Отсоединяемся от SoftAP
                                    wifiManager.DisableNetwork(robotNetworkId);//DisableNetwork предотвращает автоматичекое соединение
                                    result = IteratorTheSameAddress(ConnectToTheSameNetwork);//Подключаемся к той же сети, к которой был подключен гаджет до того, как подключили Распберри
                                }
                                else if (resultIpAddress == "NotConnected")
                                {
                                    string resultConnect = await ba.IteratorAsync(LastConnectedNetwork.Ssid, networkKey, "", ba.ConnectRobotToWiFiAsync);

                                    //string resultConnect = await ba.ConnectRobotToWiFiAsync(LastConnectedNetwork.Ssid, networkKey, "");//Посылаем запрос к Windows Device Portal API, чтобы подключить Raspberry к той же сети, к которой был подключен гаджет 
                                    if (resultConnect != "OK")
                                    {
                                        wifiManager.Disconnect();
                                        wifiManager.DisableNetwork(robotNetworkId);//DisableNetwork предотвращает автоматичекое соединение
                                        result = IteratorTheSameAddress(ConnectToTheSameNetwork);//Подключаемся к той же сети, к которой был подключен гаджет до того, как подключили Распберри
                                        mp.NotifyUser("May be the network security key for " + LastConnectedNetwork.Ssid + " is incorrect or you need to try again.", MainPage.NotifyType.ErrorMessage);
                                    }
                                    else
                                    {
                                        resultIpAddress = await ba.IteratorAsync(CommonStruct.robotNetworkPassword, CommonStruct.robotNetworkAdminName, "192.168.137.1:8080", ba.LookingForIPAddress);
                                        mp.NotifyUser("BotEyes is connected to WiFi successfully. IP address: " + CommonStruct.robotIpAddress, MainPage.NotifyType.StatusMessage);
                                        wifiManager.Disconnect();
                                        wifiManager.DisableNetwork(robotNetworkId);//DisableNetwork предотвращает автоматичекое соединение
                                        result = IteratorTheSameAddress(ConnectToTheSameNetwork);
                                    }
                                }
                                else
                                {
                                    mp.NotifyUser("I am not sure all is OK. See if robot green light diode is On. In other case try again." + CommonStruct.robotIpAddress, MainPage.NotifyType.ErrorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        mp.NotifyUser("Something is going wrong. Try again, please.", MainPage.NotifyType.ErrorMessage);
                    }
                }
                else
                {
                    mp.NotifyUser("Your gadget must be connected to WiFi net to which you want to connect the robot", MainPage.NotifyType.ErrorMessage);
                }
            }
            catch(Exception ex)
            {
                mp.NotifyUser(ex.Message, MainPage.NotifyType.ErrorMessage);
                wifiManager.Disconnect();
                wifiManager.DisableNetwork(robotNetworkId);//DisableNetwork предотвращает автоматичекое соединение
                string result = IteratorTheSameAddress(ConnectToTheSameNetwork);
            }
        }

        private string ConnectToThisNetwork(string ssid, string networkKey)
        {
            string answer = "";
            string formattedSsid = string.Format("\"{0}\"", ssid);
            string formattedPassword = string.Format("\"{0}\"", networkKey);

            var wifiConfig = new WifiConfiguration();
            wifiConfig.Ssid = formattedSsid;
            wifiConfig.PreSharedKey = formattedPassword;
            wifiConfig.StatusField = WifiStatus.Enabled;
            int networkId = wifiManager.AddNetwork(wifiConfig);
            
            wifiManager.EnableNetwork(networkId, true);
            //wifiManager.Reconnect();
            //wifiManager.Reassociate();
            if (networkId == -1)
            {
                answer = "Connection failed";
            }
            else
            {
                answer = "OK";
            }
            return answer;
        }

        private string ConnectToRaspberry()
        {
            string answer = "";
            try
            {
                    answer = ConnectToThisNetwork(SoftApSsid, "p@ssw0rd");
            }
            catch (Exception ex)
            {
                answer = ex.Message;
            }
            return answer;
        }

        private string ConnectToTheSameNetwork()
        {
            string answer = "";
            try
            {
                bool z = wifiManager.EnableNetwork(gadgetNetworkId, true);
                //wifiManager.Reconnect();//В примере Микрософта это есть
                //wifiManager.Reassociate();//В примере Микрософта этого нет
                if (z == false)
                {
                    answer = ConnectToThisNetwork(CommonStruct.lastConnectedSsid, networkKey);
                }
                else
                {
                    answer = "OK";
                }
            }
            catch(Exception ex)
            {
                answer = ex.Message;
            }
            return answer;
        }

        private string IteratorTheSameAddress(FunctionTheSameAddress func)
        {
            string result = "";
            DateTime dt = DateTime.Now;
            long startTicks = dt.Ticks;
            long oneSec = 10000000;//Одна сек = 10 млн. тиков
            MessageFromRobot netAvailability = new MessageFromRobot();
            result =  func();
            long timeEllapsed = 0;

            if ((result != "OK") && (result != "NotConnected"))
            {
                while (((result != "OK") && (result != "NotConnected")) && (timeEllapsed < 120))
                {
                    Thread.Sleep(500);
                    result =  func();
                    dt = DateTime.Now;
                    timeEllapsed = (dt.Ticks - startTicks) / oneSec;//
                    if (timeEllapsed > 10)
                    {
                        mp.NotifyUser("Check if the robot is ON. May be you need to reboot it.", MainPage.NotifyType.ErrorMessage);
                        timeEllapsed = 0;
                        break;
                    }
                }
            }
            return result;
        }

        public void OnGrantedPermission(int requestCode)
        {
            if (requestCode == ACCESS_FINE_LOCATION)
            {
                permissions = "Yes";
                AccessPointHelper = new WifiHelper();
                AccessPointHelper.AccessPointsEnumeratedEvent += AccessPointHelper_AccessPointsEnumeratedEvent;
                FindAccessPoints(null, null);
            }
        }

        public void OnDeniedPermission(int requestCode)
        {
            if (requestCode == ACCESS_FINE_LOCATION)
            {
                permissions = "No";
                owner.ShowToast("Permission was denied");
                mp.NotifyUser("Cannot connect to WiFi without Permission 'Location' for BotEyesWiFi app, see User Manual.", MainPage.NotifyType.ErrorMessage);
            }
        }
    }
}
