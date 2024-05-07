using Android.Locations;
using Firebase.Database;
using GoogleGson.Annotations;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using Plugin.Firebase.CloudMessaging;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace CalculatorProj.Pages;

public partial class HistoryAndTheme : ContentPage
{
    private Color _color = Color.FromRgb(255, 0, 0);
    public ObservableCollection<string> Expressions { get; set; } = new();

    private FirebaseClient _firebaseClient = new("https://calculatorproj-4b58f-default-rtdb.europe-west1.firebasedatabase.app/");

    public HistoryAndTheme()
	{
        BindingContext = this;

        try
        {
            var collection = _firebaseClient.Child("Expressions").AsObservable<string>().Subscribe(item =>
            {
                if (item.Object is string str)
                {
                    Expressions.Add(str);
                }
            });

            _firebaseClient.Child("Theme").PostAsync(JsonSerializer.Serialize("test")).Wait();
            var node = _firebaseClient.Child("Theme").OnceSingleAsync<object>().Result;
        }
        catch { }

        InitializeComponent();

        themeBtn.BackgroundColor = _color;
        progBar.ProgressColor = _color;
	}

    private void CopyButton_Clicked(object sender, EventArgs e)
    {
        string textToCopy = myLabel.Text;
        Clipboard.SetTextAsync(textToCopy);
    }

    private async void GetToken_Clicked(object sender, EventArgs e)
    {
        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        string token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

        myLabel.Text = token;
    }

    private async void Fingerprint_Clicked(object sender, EventArgs e)
    {
        bool isAvailable = await CrossFingerprint.Current.IsAvailableAsync();

        if (isAvailable)
        {
            var request = new AuthenticationRequestConfiguration("Авторизация отпечатком", "Авторизируйтесь при помощи отпечатка пальца");

            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            if (result.Authenticated)
            {
                await DisplayAlert("Авторизация", "Авторизация прошла успешно", "Ok");
            }
            else
            {
                await DisplayAlert("Авторизация", "Авторизация провалена", "Ok");
            }
        }
    }

    private async void ChangeTheme_Clicked(object sender, EventArgs e)
    {
        if (_color.Red == 1)
        {
            _color = new(0, 0, 255);
        }
        else
        {
            _color = new(255, 0, 0);
        }

        try
        {
            await _firebaseClient.Child("Theme").PutAsync(JsonSerializer.Serialize(_color));
        }
        catch { }

        themeBtn.BackgroundColor = _color;
        progBar.ProgressColor = _color;
    }

    private async void StartProgress_Clicked(object sender, EventArgs e)
    {
        progBar.Progress = 0;

        for (int i = 0; i < 10; i++)
        {
            progBar.Progress += 0.1;
            await Task.Delay(1000);
        }
    }
}