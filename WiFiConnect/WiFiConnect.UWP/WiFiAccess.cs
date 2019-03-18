using WiFiConnect.UWP;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.UI.Popups;
using Xamarin.Forms;
using System.Threading;
using System.ComponentModel;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using WiFiConnect;


[assembly: Dependency(typeof(WiFiConnect.UWP.WiFiAccess))]
namespace WiFiConnect.UWP
{
    delegate string FunctionTheSameAddress();

    public class WiFiAccess : WiFiConnect.IWiFiInterface
    {
        WiFiConnect.MainPage mp;
        int robotNetworkId = -1;
        int gadgetNetworkId = -1;
        WiFiConnect.BasicAlgorthm ba;
        string networkKey = "";

        public WiFiAccess()
        {
            mp = WiFiConnect.MainPage.mainPage;
            ba = new WiFiConnect.BasicAlgorthm();
        }

        public void ConnectToWifi(string networkKey)
        {
            
        }









        private string IteratorTheSameAddress(FunctionTheSameAddress func)
        {
            string result = "";
            DateTime dt = DateTime.Now;
            long startTicks = dt.Ticks;
            long oneSec = 10000000;//Одна сек = 10 млн. тиков
            MessageFromRobot netAvailability = new MessageFromRobot();
            result = func();
            long timeEllapsed = 0;

            if ((result != "OK") && (result != "NotConnected"))
            {
                while (((result != "OK") && (result != "NotConnected")) && (timeEllapsed < 120))
                {
                    Thread.Sleep(500);
                    result = func();
                    dt = DateTime.Now;
                    timeEllapsed = (dt.Ticks - startTicks) / oneSec;//
                    if (timeEllapsed > 10)
                    {
                        //mp.NotifyUser("Check if the robot is ON. May be you need to reboot it.", MainPage.NotifyType.ErrorMessage);
                        timeEllapsed = 0;
                        break;
                    }
                }
            }
            return result;
        }
    }
}