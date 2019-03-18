using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WiFiConnect.Droid
{
    public interface IAccessPointHelper
    {
        event Action<string> AccessPointsEnumeratedEvent;
        void FindAccessPoints(ObservableCollection<AccessPoint> availableAccessPoints);
    }
}
