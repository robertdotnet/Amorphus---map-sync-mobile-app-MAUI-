using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavPoint.Core.Models;
using NavPoint.Core.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NavPoint.Core.ViewModels;

[ObservableObject]
public partial class Locations
{

    [ObservableProperty]
    private ObservableCollection<LocationUnit> _locationUnits;

    int count = 0;

    public Locations()
    {
        LocationUnits = new ObservableCollection<LocationUnit>();
        LoadCurrentLocations();
        //NavigateCommand = new RelayCommand(OnClick(LocationUnits.First()), CanExecuteNavigateButton);
    }

    [RelayCommand]
    private void LoadCurrentLocations()
    {
        var locations = new ObservableCollection<LocationUnit>();

        locations.Add(new LocationUnit("Home", LocationType.Place, new Coordinates() { XCoordinate = 44.443145, YCoordinate = 26.022156 }));
        locations.Add(new LocationUnit("Work", LocationType.Place, new Coordinates() { XCoordinate = 44.481202, YCoordinate = 26.115779 }));
        locations.Add(new LocationUnit("Stefania", LocationType.People, new Coordinates() { XCoordinate = 44.435427, YCoordinate = 26.023330 }));
        locations.Add(new LocationUnit("Rehearsals", LocationType.People, new Coordinates() { XCoordinate = 44.435427, YCoordinate = 26.023330 }));
        locations.Add(new LocationUnit("TDA", LocationType.People, new Coordinates() { XCoordinate = 44.435427, YCoordinate = 26.023330 }));

        //TODO: logic from source e.g. DB

        LocationUnits = locations;
    }

    [RelayCommand]
    private void Add(LocationUnit locationUnit)
    {
        //TODO: validation
        LocationUnits.Add(locationUnit);
    }


    public ICommand Navigate =>
        new Command(NavigateEvent);

    private void NavigateEvent(object obj)
    {
        Application.Current.MainPage.DisplayAlert("navigation", "clicked navigation", "OK");
    }

}
