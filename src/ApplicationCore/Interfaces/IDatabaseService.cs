using System.Data.Common;

namespace ApplicationCore.Interfaces;

public interface IDatabaseService {
    public Task InitializeAsync();
    public Task<int> NonQueryAsync(string commandText);
    public Task<DbDataReader> QueryAsync(string queryText);
}