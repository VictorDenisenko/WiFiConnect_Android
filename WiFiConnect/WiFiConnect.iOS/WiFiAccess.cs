using Foundation;
using WiFiConnect.iOS;
using UIKit;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Threading;
using NetworkExtension;

[assembly: Dependency(typeof(WiFiAccess))]
namespace WiFiConnect.iOS
{
    delegate string FunctionTheSameAddress();

    public class WiFiAccess : IWiFiInterface
    {
        MainPage mp;
        BasicAlgorthm ba;
        
        string networkKey = "";

        public WiFiAccess()
        {
            mp = MainPage.mainPage;
            ba = new BasicAlgorthm();
        }


        public async void ConnectToWifi(string _networkKey)
        {
            networkKey = _networkKey;
            string result = await ba.IteratorAsync(CommonStruct.robotNetworkPassword, CommonStruct.robotNetworkAdminName, "", ba.LookingForInfo);
            if (result != "OK")
            {
                mp.NotifyUser("Something is wrong. May be you need to reboot the robot...", MainPage.NotifyType.ErrorMessage);
            }
            else
            {
                string resultIpAddress = await ba.IteratorAsync(CommonStruct.robotNetworkPassword, CommonStruct.robotNetworkAdminName, "192.168.137.1:8080", ba.LookingForIPAddress);
                if (resultIpAddress == "OK")
                {
                    mp.NotifyUser("Your robot is already connected to WiFi. IP address is " + CommonStruct.robotIpAddress, MainPage.NotifyType.StatusMessage);
                }
                else if (resultIpAddress == "NotConnected")
                {//
                    string resultConnect = await ba.IteratorAsync("RLDA_NET", networkKey, "", ba.ConnectRobotToWiFiAsync);
                    resultIpAddress = await ba.IteratorAsync(CommonStruct.robotNetworkPassword, CommonStruct.robotNetworkAdminName, "192.168.137.1:8080", ba.LookingForIPAddress);
                    mp.NotifyUser("BotEyes is connected to WiFi successfully. IP address: " + CommonStruct.robotIpAddress, MainPage.NotifyType.StatusMessage);
                }
                else
                {
                    mp.NotifyUser("I am not sure all is OK. See if robot green light diode is On. In other case try again." + CommonStruct.robotIpAddress, MainPage.NotifyType.ErrorMessage);
                }
            }
        }
    }
}