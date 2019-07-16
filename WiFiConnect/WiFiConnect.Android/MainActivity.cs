using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using Xamarin.Forms;


namespace WiFiConnect.Droid
{
    [Activity(Label = "WiFiConnect", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }
        MainPage mp;
        public static MainActivity owner;

        //public interface IPermissionCallback
        //{
        //    void OnGrantedPermission(int requestCode);
        //    void OnDeniedPermission(int requestCode);
        //}

        public MainActivity()
        {
            owner = this;
        }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);
            Instance = this;
            Forms.Init(this, bundle);
            LoadApplication(new App());

            mp = MainPage.mainPage;
        }

        public void ShowToast(string message)
        {
            Toast.MakeText(this, message, ToastLength.Long).Show();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}

