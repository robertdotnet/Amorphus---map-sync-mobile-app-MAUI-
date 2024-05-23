using NavPoint.Core.Models;
using NavPoint.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavPoint.Core.ViewModels;

public class Locations
{
    public List<LocationUnit> LocationUnits {  get; set; }

    public Locations()
    {
        LocationUnits = new List<LocationUnit>();
    }

    public List<LocationUnit> LoadLocations()
    {
        var locations = new List<LocationUnit>();

        locations.AddRange(new List<LocationUnit>()
        {
            new LocationUnit("Home", LocationType.Place, new Coordinates() { XCoordinate = 44.443145, YCoordinate = 26.022156 }),
            new LocationUnit("Work", LocationType.Place, new Coordinates() { XCoordinate = 44.481202, YCoordinate = 26.115779 }),
            new LocationUnit("Stefania", LocationType.People, new Coordinates() { XCoordinate = 44.435427, YCoordinate = 26.023330 })
        });
        //TODO: logic from source e.g. DB

        return locations;
    }
}
