using System.Threading.Tasks;

namespace WiFiConnect
{
    public interface IWiFiInterface
    {
        void ConnectToWifi(string networkKey);
    }
}