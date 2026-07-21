using NavPoint.Core.Models;
using NavPoint.Core.ViewModels;

namespace NavPoint;

public partial class MainPage : ContentPage
{
    private readonly Locations _viewModel;

    public MainPage(Locations viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: LocationUnit location })
        {
            return;
        }

        var shouldDelete = await DisplayAlert(
            "Delete entry?",
            $"Remove “{location.LocationName}” from your saved destinations?",
            "Delete",
            "Cancel");

        if (shouldDelete)
        {
            await _viewModel.DeleteCommand.ExecuteAsync(location);
        }
    }
}
