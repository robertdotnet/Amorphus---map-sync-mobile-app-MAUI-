using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavPoint.Core.Data;
using NavPoint.Core.Models;
using NavPoint.Core.Types;

namespace NavPoint.Core.ViewModels;

public partial class Locations : ObservableObject
{
    private readonly ILocationRepository _repository;
    private int? _editingLocationId;
    private bool _isInitialized;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFailureMessage))]
    private string _failureMessage = string.Empty;

    [ObservableProperty]
    private bool _isEditorOpen;

    [ObservableProperty]
    private string _editorTitle = "Add entry";

    [ObservableProperty]
    private string _editorName = string.Empty;

    [ObservableProperty]
    private LocationType _editorLocationType = LocationType.Place;

    [ObservableProperty]
    private string _editorLatitude = string.Empty;

    [ObservableProperty]
    private string _editorLongitude = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasValidationMessage))]
    private string _validationMessage = string.Empty;

    public Locations(ILocationRepository repository)
    {
        _repository = repository;
    }

    public ObservableCollection<LocationUnit> LocationUnits { get; } = [];

    public IReadOnlyList<LocationType> LocationTypes { get; } = Enum.GetValues<LocationType>();

    public bool IsNotBusy => !IsBusy;

    public bool IsEmpty => !IsBusy && LocationUnits.Count == 0;

    public bool HasFailureMessage => !string.IsNullOrWhiteSpace(FailureMessage);

    public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task InitializeAsync()
    {
        if (_isInitialized || IsBusy)
        {
            return;
        }

        IsBusy = true;
        FailureMessage = string.Empty;

        try
        {
            var locations = await _repository.GetAllAsync();
            LocationUnits.Clear();
            foreach (var location in locations)
            {
                LocationUnits.Add(location);
            }

            _isInitialized = true;
        }
        catch (Exception)
        {
            FailureMessage = "Your saved entries could not be loaded. Please restart the app and try again.";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    [RelayCommand]
    private void OpenAdd()
    {
        _editingLocationId = null;
        EditorTitle = "Add entry";
        EditorName = string.Empty;
        EditorLocationType = LocationType.Place;
        EditorLatitude = string.Empty;
        EditorLongitude = string.Empty;
        ValidationMessage = string.Empty;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void OpenEdit(LocationUnit location)
    {
        ArgumentNullException.ThrowIfNull(location);

        _editingLocationId = location.Id;
        EditorTitle = "Edit entry";
        EditorName = location.LocationName;
        EditorLocationType = location.LocationType;
        EditorLatitude = location.Latitude.ToString("G17", CultureInfo.InvariantCulture);
        EditorLongitude = location.Longitude.ToString("G17", CultureInfo.InvariantCulture);
        ValidationMessage = string.Empty;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void CloseEditor()
    {
        IsEditorOpen = false;
        ValidationMessage = string.Empty;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task SaveAsync()
    {
        if (!TryBuildLocation(out var location, out var validationMessage))
        {
            ValidationMessage = validationMessage;
            return;
        }

        IsBusy = true;
        FailureMessage = string.Empty;

        try
        {
            if (_editingLocationId is int locationId)
            {
                location.Id = locationId;
                await _repository.UpdateAsync(location);

                var index = LocationUnits
                    .Select((item, itemIndex) => new { item.Id, Index = itemIndex })
                    .First(pair => pair.Id == locationId)
                    .Index;
                LocationUnits[index] = location;
            }
            else
            {
                await _repository.AddAsync(location);
                LocationUnits.Add(location);
            }

            IsEditorOpen = false;
            ValidationMessage = string.Empty;
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception)
        {
            const string message = "The entry could not be saved. Please try again.";
            FailureMessage = message;
            ValidationMessage = message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task DeleteAsync(LocationUnit location)
    {
        ArgumentNullException.ThrowIfNull(location);

        IsBusy = true;
        FailureMessage = string.Empty;

        try
        {
            await _repository.DeleteAsync(location);
            LocationUnits.Remove(location);
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception)
        {
            FailureMessage = "The entry could not be deleted. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool TryBuildLocation(out LocationUnit location, out string validationMessage)
    {
        location = new LocationUnit();
        validationMessage = string.Empty;
        var name = EditorName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            validationMessage = "Enter a name.";
            return false;
        }

        if (name.Length > 120)
        {
            validationMessage = "Name must be 120 characters or fewer.";
            return false;
        }

        if (!TryParseCoordinate(EditorLatitude, out var latitude) ||
            !double.IsFinite(latitude) ||
            latitude is < -90 or > 90)
        {
            validationMessage = "Enter a latitude between -90 and 90.";
            return false;
        }

        if (!TryParseCoordinate(EditorLongitude, out var longitude) ||
            !double.IsFinite(longitude) ||
            longitude is < -180 or > 180)
        {
            validationMessage = "Enter a longitude between -180 and 180.";
            return false;
        }

        location = new LocationUnit
        {
            LocationName = name,
            LocationType = EditorLocationType,
            Latitude = latitude,
            Longitude = longitude,
        };
        return true;
    }

    private static bool TryParseCoordinate(string value, out double coordinate) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out coordinate) ||
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out coordinate);
}
