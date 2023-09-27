using Android.App;
using Android.Runtime;

[assembly: UsesPermission(Android.Manifest.Permission.Internet)]

// Needed for Picking photo
[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage, MaxSdkVersion = 32)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadMediaImages)]

// Needed for Taking photo
[assembly: UsesPermission(Android.Manifest.Permission.Camera)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage, MaxSdkVersion = 32)]

namespace Carmen.Mobile
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}