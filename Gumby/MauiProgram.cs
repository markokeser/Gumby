using Microsoft.Extensions.Logging;

namespace Gumby
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            ConfigureControlAppearance();

            return builder.Build();
        }

        /// <summary>
        /// Uklanja Android donju liniju sa polja za unos - okvir crtaju naši Border stilovi (InputShell).
        /// </summary>
        private static void ConfigureControlAppearance()
        {
#if ANDROID
            var transparent = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("GumbyNoUnderline",
                (handler, _) => handler.PlatformView.BackgroundTintList = transparent);

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("GumbyNoUnderline",
                (handler, _) => handler.PlatformView.BackgroundTintList = transparent);

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("GumbyNoUnderline",
                (handler, _) => handler.PlatformView.BackgroundTintList = transparent);

            Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("GumbyNoUnderline", (handler, _) =>
            {
                var view = handler.PlatformView;
                var plateId = view.Context?.Resources?.GetIdentifier("search_plate", "id", "android") ?? 0;
                if (plateId != 0)
                    view.FindViewById(plateId)?.SetBackgroundColor(Android.Graphics.Color.Transparent);
            });
#endif
        }
    }
}