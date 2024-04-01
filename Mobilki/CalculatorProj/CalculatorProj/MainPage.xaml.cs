using CalculatorProj.Pages;
using Microsoft.Maui.Controls.Xaml;

namespace CalculatorProj
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        }

        private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            string visualState = e.DisplayInfo.Orientation == DisplayOrientation.Landscape ? "Vertical" : "Horizontal";
            VisualStateManager.GoToState(buttons, visualState);
        }

    }

}
