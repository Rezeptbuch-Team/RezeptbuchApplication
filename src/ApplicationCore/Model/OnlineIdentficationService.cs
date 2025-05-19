using System.Data.Common;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class OnlineIdentificationService(IDatabaseService databaseService) : IOnlineIdentificationService
{
    /// <summary>
    /// Get the UUID from the database
    /// </summary>
    /// <returns>UUID</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string?> GetUUID()
    {
        string sql = "SELECT value FROM app_info WHERE key = 'uuid';";

        string uuid = "";
        await using (DbDataReader resultReader = await databaseService.QueryAsync(sql))
        {
            if (await resultReader.ReadAsync())
            {
                uuid = resultReader.GetString(0);
            }
        }

        return string.IsNullOrWhiteSpace(uuid) ? null : uuid;
    }

    /// <summary>
    /// If there is no UUID yet one gets created
    /// </summary>
    /// <returns>UUID</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> CreateUUID()
    {
        string? uuid = await GetUUID();
        if (uuid != null) return uuid;

        // generate a uuid
        uuid = Guid.NewGuid().ToString();
        #region insert uuid into database
        string sql = "INSERT INTO app_info (key, value) VALUES ($key, $value);";
        Dictionary<string, object> parameters = new()
        {
            ["$key"] = "uuid",
            ["$value"] = uuid
        };
        if (await databaseService.NonQueryAsync(sql, parameters) == 0) throw new Exception("Error inserting uuid into database");
        #endregion

        return uuid;
    }
}