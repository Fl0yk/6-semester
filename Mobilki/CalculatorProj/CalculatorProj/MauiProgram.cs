using CalculatorProj.Models.Implementations;
using CalculatorProj.Models.Interfaces;
using CalculatorProj.Pages;
using CalculatorProj.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace CalculatorProj
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            #region Dependency injection
            builder.Services.AddTransient<IEngineeringCalculator<double>, DoubleEngineeringCalculator>();
            builder.Services.AddTransient<IEngineeringCalculator<decimal>, DecimalEngineeringCalculator>();
            builder.Services.AddTransient<CalculatorViewModel>();
            builder.Services.AddTransient<CalculatorPage>();

            #endregion

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
#if ANDROID
            Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("WhiteBackground", (handler, view) =>
            {
                var button = view.Handler.PlatformView as AndroidX.AppCompat.Widget.AppCompatButton;
                button.SetBackgroundColor(Android.Graphics.Color.MediumPurple);
                button.SetTextColor(Android.Graphics.Color.White);
            });
#endif
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
