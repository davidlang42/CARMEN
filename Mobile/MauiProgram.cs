using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Carmen.Mobile
{
    public static class MauiProgram
    {
        // Some weird bug with EFCore6 and net8 causes LazyLoadProxies to crash with InvalidCastException in PreloadModel (worked fine in EFCore5 and net7)
        // The most similar bug I could find was: https://github.com/dotnet/efcore/issues/26602
        // But in this case it crashes in: Castle.DynamicProxy.Internal.AttributeUtil.ReadAttributeValue(CustomAttributeTypedArgument argument)
        // And the most weird part- ONLY IN ANDROID (but works in Windows)
        // So the best I could manage was to disable LazyLoadProxies in Android.
#if !ANDROID
        public const bool USE_LAZY_LOAD_PROXIES = true;
#else
        public const bool USE_LAZY_LOAD_PROXIES = false;
#endif

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}