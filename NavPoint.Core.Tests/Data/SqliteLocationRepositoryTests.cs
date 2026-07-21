using NavPoint.Core.Data;
using NavPoint.Core.Models;
using NavPoint.Core.Types;
using Xunit;

namespace NavPoint.Core.Tests.Data;

public sealed class SqliteLocationRepositoryTests
{
    [Fact]
    public async Task InitializeAsync_SeedsCurrentLocations()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using var repository = new SqliteLocationRepository(databasePath);

            var locations = await repository.GetAllAsync();

            Assert.Collection(
                locations,
                location => Assert.Equal("Home", location.LocationName),
                location => Assert.Equal("Work", location.LocationName),
                location => Assert.Equal("TDA", location.LocationName),
                location => Assert.Equal("Andrei Mihailescu", location.LocationName),
                location => Assert.Equal("Georgi (profa)", location.LocationName));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task InitializeAsync_DoesNotDuplicateSeedsOnLaterLaunches()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using (var firstLaunch = new SqliteLocationRepository(databasePath))
            {
                await firstLaunch.InitializeAsync();
            }

            await using var laterLaunch = new SqliteLocationRepository(databasePath);
            await laterLaunch.InitializeAsync();

            Assert.Equal(5, (await laterLaunch.GetAllAsync()).Count);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task InitializeAsync_DoesNotReseedAfterAllLocationsAreDeleted()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using (var firstLaunch = new SqliteLocationRepository(databasePath))
            {
                foreach (var location in await firstLaunch.GetAllAsync())
                {
                    await firstLaunch.DeleteAsync(location);
                }
            }

            await using var laterLaunch = new SqliteLocationRepository(databasePath);

            Assert.Empty(await laterLaunch.GetAllAsync());
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AddAsync_PersistsLocation()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using var repository = new SqliteLocationRepository(databasePath);
            var location = new LocationUnit(
                "Coffee shop",
                LocationType.Place,
                new Coordinates { XCoordinate = 44.45, YCoordinate = 26.1 });

            await repository.AddAsync(location);
            var saved = (await repository.GetAllAsync()).Single(item => item.Id == location.Id);

            Assert.Equal("Coffee shop", saved.LocationName);
            Assert.Equal(44.45, saved.Coordinates.XCoordinate);
            Assert.Equal(26.1, saved.Coordinates.YCoordinate);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using var repository = new SqliteLocationRepository(databasePath);
            var location = (await repository.GetAllAsync()).First();
            location.LocationName = "New home";
            location.LocationType = LocationType.Custom;
            location.Latitude = 45.1;
            location.Longitude = 25.2;

            await repository.UpdateAsync(location);
            var updated = (await repository.GetAllAsync()).Single(item => item.Id == location.Id);

            Assert.Equal("New home", updated.LocationName);
            Assert.Equal(LocationType.Custom, updated.LocationType);
            Assert.Equal(45.1, updated.Latitude);
            Assert.Equal(25.2, updated.Longitude);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesLocation()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using var repository = new SqliteLocationRepository(databasePath);
            var location = (await repository.GetAllAsync()).First();

            await repository.DeleteAsync(location);

            Assert.DoesNotContain(await repository.GetAllAsync(), item => item.Id == location.Id);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static string CreateDatabasePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "NavPoint.Tests");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"{Guid.NewGuid():N}.db3");
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        File.Delete(databasePath);
        File.Delete($"{databasePath}-shm");
        File.Delete($"{databasePath}-wal");
    }
}
