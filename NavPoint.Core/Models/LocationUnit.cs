using NavPoint.Core.Types;

namespace NavPoint.Core.Models;

// All the code in this file is included in all platforms.
public class LocationUnit
{
    public string LocationName { get; set; }
    public LocationType LocationType { get; set; }
    public Coordinates Coordinates { get; set; }

    public LocationUnit()
    { }

    public LocationUnit(string locationName, LocationType locationType, Coordinates coordinates)
    {
        LocationName = locationName;
        LocationType = locationType;
        Coordinates = coordinates;
    }
}
