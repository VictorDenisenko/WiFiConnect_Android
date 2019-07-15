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

        public interface IPermissionCallback
        {
            void OnGrantedPermission(int requestCode);
            void OnDeniedPermission(int requestCode);
        }

        int currentRequestCodePermission;
        //IPermissionCallback currentPermissionCallback;

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

        //public void AskForPermission(string permission, int requestCode, IPermissionCallback callback)
        //{
        //    currentPermissionCallback = callback;
        //    var permissionCheck = (int)ContextCompat.CheckSelfPermission(this, permission);
        //    if (permissionCheck == (int)Permission.Granted)
        //    {
        //        callback.OnGrantedPermission(requestCode);
        //    }
        //    else
        //    {
        //        currentRequestCodePermission = requestCode;
        //        ActivityCompat.RequestPermissions(this, new string[] { permission }, requestCode);
        //    }
        //}

        public void ShowToast(string message)
        {
            Toast.MakeText(this, message, ToastLength.Long).Show();
        }

        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        //{
        //    if (requestCode == currentRequestCodePermission)
        //    {
        //        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
        //        {
        //            if (currentPermissionCallback != null)
        //            {
        //                currentPermissionCallback.OnGrantedPermission(requestCode);
        //            }
        //        }
        //        else
        //        {
        //            if (currentPermissionCallback != null)
        //            {
        //                currentPermissionCallback.OnDeniedPermission(requestCode);
        //            }
        //        }
        //        currentPermissionCallback = null;
        //    }
        //}

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}

