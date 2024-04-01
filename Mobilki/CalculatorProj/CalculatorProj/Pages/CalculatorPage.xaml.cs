using Microsoft.Maui.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CalculatorProj.Pages;

public partial class CalculatorPage : ContentPage
{
	public CalculatorPage()
    {
		InitializeComponent();
        VisualStateManager.GoToState(buttons, "Vertical");

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
    }

    private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        string visualState = e.DisplayInfo.Orientation == DisplayOrientation.Portrait ? "Vertical" : "Horizontal";
        VisualStateManager.GoToState(buttons, visualState);
    }

    private void OnButtonDigitClicked(object sender, System.EventArgs e)
    {
        Button button = (Button)sender;
        
    }

    private void OnButtonBinaryOpClicked(object sender, System.EventArgs e)
    {
        Button button = (Button)sender;
        
    }

    private void OnButtonEqualsClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonLogClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonReverseClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonChangeSignClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonPow2Clicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonSqrtClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonClearOneClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonClearAllClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonClearInputClicked(object sender, System.EventArgs e)
    {
        
    }

    private void OnButtonAddCommaClicked(object sender, System.EventArgs e)
    {
        
    }
}