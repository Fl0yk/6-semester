using CalculatorProj.ViewModels;

namespace CalculatorProj.Pages;

public partial class CalculatorPage : ContentPage
{
    private List<Button> buttons = [];
	public CalculatorPage(CalculatorViewModel calculatorVM)
    {
		InitializeComponent();
        
        this.BindingContext = calculatorVM;//new CalculatorViewModel(new DoubleEngineeringCalculator());

        int i = 1;

        while (FindByName("but" + i) is Button but)
        {
            buttons.Add(but);
            i++;
        }
        
#if ANDROID
        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
#endif
    }

    private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        string visualState = e.DisplayInfo.Orientation == DisplayOrientation.Portrait ? "Vertical" : "Horizontal";

        foreach (var b in buttons)
            VisualStateManager.GoToState(b, visualState);
    }
}