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
            builder.Services.AddTransient<IEngineeringCalculator<Java.Math.BigDecimal>, JavaEngineeringCalculator>();
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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
