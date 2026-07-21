using NavPoint.Core.Models;
using NavPoint.Core.Types;
using SQLite;

namespace NavPoint.Core.Data;

public sealed class SqliteLocationRepository : ILocationRepository
{
    private const string SeedCompletedKey = "initial-locations-seeded";

    private static readonly IReadOnlyList<LocationUnit> InitialLocations =
    [
        new("Home", LocationType.Place, new Coordinates { XCoordinate = 44.443145, YCoordinate = 26.022156 }),
        new("Work", LocationType.Place, new Coordinates { XCoordinate = 44.481202, YCoordinate = 26.115779 }),
        new("TDA", LocationType.Place, new Coordinates { XCoordinate = 44.436049, YCoordinate = 26.034825 }),
        new("Andrei Mihailescu", LocationType.People, new Coordinates { XCoordinate = 44.466632, YCoordinate = 26.031133 }),
        new("Georgi (profa)", LocationType.People, new Coordinates { XCoordinate = 44.484323, YCoordinate = 26.038668 }),
    ];

    private readonly SQLiteAsyncConnection _database;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;

    public SqliteLocationRepository(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _database = new SQLiteAsyncConnection(
            databasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_isInitialized)
            {
                return;
            }

            await _database.CreateTableAsync<LocationUnit>();
            await _database.CreateTableAsync<DatabaseMetadata>();

            await _database.RunInTransactionAsync(connection =>
            {
                var hasSeeded = connection.Find<DatabaseMetadata>(SeedCompletedKey) is not null;
                if (hasSeeded)
                {
                    return;
                }

                foreach (var location in InitialLocations)
                {
                    connection.Insert(Clone(location));
                }

                connection.Insert(new DatabaseMetadata
                {
                    Key = SeedCompletedKey,
                    Value = DateTimeOffset.UtcNow.ToString("O"),
                });
            });

            _isInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<IReadOnlyList<LocationUnit>> GetAllAsync()
    {
        await InitializeAsync();
        return await _database.Table<LocationUnit>().OrderBy(location => location.Id).ToListAsync();
    }

    public async Task AddAsync(LocationUnit location)
    {
        ArgumentNullException.ThrowIfNull(location);
        await InitializeAsync();
        await _database.InsertAsync(location);
    }

    public async Task UpdateAsync(LocationUnit location)
    {
        ArgumentNullException.ThrowIfNull(location);
        await InitializeAsync();

        var rowsUpdated = await _database.UpdateAsync(location);
        if (rowsUpdated == 0)
        {
            throw new InvalidOperationException($"Location {location.Id} no longer exists.");
        }
    }

    public async Task DeleteAsync(LocationUnit location)
    {
        ArgumentNullException.ThrowIfNull(location);
        await InitializeAsync();
        await _database.DeleteAsync(location);
    }

    public async ValueTask DisposeAsync()
    {
        await _database.CloseAsync();
        _initializationLock.Dispose();
    }

    private static LocationUnit Clone(LocationUnit location) => new()
    {
        LocationName = location.LocationName,
        LocationType = location.LocationType,
        Latitude = location.Latitude,
        Longitude = location.Longitude,
    };

    [Table("Metadata")]
    private sealed class DatabaseMetadata
    {
        [PrimaryKey]
        public string Key { get; set; } = string.Empty;

        [NotNull]
        public string Value { get; set; } = string.Empty;
    }
}
