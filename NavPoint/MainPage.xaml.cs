using NavPoint.Core.ViewModels;

namespace NavPoint;

public partial class MainPage : ContentPage
{
    int count = 1;
    public MainPage()
    {
        InitializeComponent();

        LocationsCollectionView.ItemsSource = new Locations().LocationUnits;
        
    }

    private void Button_Delete_Clicked(object sender, EventArgs e)
    {
        testLabel.Text = $"clicked {count}";
        count++;

        DisplayAlert("delete",$"{sender}", "ok");
    }
}
