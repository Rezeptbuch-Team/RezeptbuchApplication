using System.Data.Common;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class CheckForPublishableUpdates(IDatabaseService databaseService) : ICheckForPublishableUpdates
{
    public async Task<List<PublishableUpdate>> GetPublishableUpdates()
    {
        List<PublishableUpdate> publishableUpdates = [];
        string sql = @"SELECT hash, title FROM recipes
                        WHERE is_modified = 1 AND is_published = 1;";
        await using (DbDataReader reader = await databaseService.QueryAsync(sql))
        {
            while (await reader.ReadAsync())
            {
                if (reader.GetValue(0) is string hash && reader.GetValue(1) is string title)
                {
                    publishableUpdates.Add(new PublishableUpdate(hash, title));
                }
            }
        }
        return publishableUpdates;
    }
}