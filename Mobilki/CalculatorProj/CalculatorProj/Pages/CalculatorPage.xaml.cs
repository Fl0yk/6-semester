using CalculatorProj.ViewModels;

namespace CalculatorProj.Pages;

public partial class CalculatorPage : ContentPage
{
	public CalculatorPage(CalculatorViewModel calculatorVM)
    {
		InitializeComponent();

        this.BindingContext = calculatorVM;//new CalculatorViewModel(new DoubleEngineeringCalculator());

#if ANDROID
        VisualStateManager.GoToState(buttons, "Vertical");

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
#endif
    }

    private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        string visualState = e.DisplayInfo.Orientation == DisplayOrientation.Portrait ? "Vertical" : "Horizontal";
        VisualStateManager.GoToState(buttons, visualState);
    }

    
}