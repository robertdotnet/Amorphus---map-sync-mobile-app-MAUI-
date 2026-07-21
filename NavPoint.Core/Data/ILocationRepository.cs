using NavPoint.Core.Models;

namespace NavPoint.Core.Data;

public interface ILocationRepository : IAsyncDisposable
{
    Task InitializeAsync();

    Task<IReadOnlyList<LocationUnit>> GetAllAsync();

    Task AddAsync(LocationUnit location);

    Task UpdateAsync(LocationUnit location);

    Task DeleteAsync(LocationUnit location);
}
