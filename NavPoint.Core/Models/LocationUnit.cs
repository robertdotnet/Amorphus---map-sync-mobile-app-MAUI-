using NavPoint.Core.Types;
using SQLite;

namespace NavPoint.Core.Models;

[Table("Locations")]
public class LocationUnit
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(120), NotNull]
    public string LocationName { get; set; } = string.Empty;

    public LocationType LocationType { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    [Ignore]
    public Coordinates Coordinates
    {
        get => new()
        {
            XCoordinate = Latitude,
            YCoordinate = Longitude,
        };
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Latitude = value.XCoordinate;
            Longitude = value.YCoordinate;
        }
    }

    public LocationUnit()
    {
    }

    public LocationUnit(string locationName, LocationType locationType, Coordinates coordinates)
    {
        LocationName = locationName;
        LocationType = locationType;
        Coordinates = coordinates;
    }
}
