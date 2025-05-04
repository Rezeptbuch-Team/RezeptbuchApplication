using System.Data.Common;

namespace ApplicationCore.Interfaces;

public interface IDatabaseService {
    public Task InitializeAsync();
    public Task<int> NonQueryAsync(string commandText, IDictionary<string, object>? parameters = null);
    public Task<DbDataReader> QueryAsync(string queryText, IDictionary<string, object>? parameters = null);
}