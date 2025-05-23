using System.Data.Common;
using System.Text;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class UploadService(IDatabaseService databaseService, HttpClient httpClient)
{
    public async Task<(string uuid, string xmlContent)> GetXmlFile(string hash)
    {
        string sql = @"SELECT r.file_path, (
                            SELECT value FROM app_info WHERE key = 'uuid'
                        ) AS uuid
                        FROM recipes r
                        WHERE r.hash = $hash;";
        Dictionary<string, object> parameters = new()
        {
            ["$hash"] = hash
        };

        string filePath = "";
        string? uuid = "";
        await using (DbDataReader reader = await databaseService.QueryAsync(sql, parameters))
        {
            if (await reader.ReadAsync())
            {
                try
                {
                    filePath = reader.GetString(0);
                    uuid = reader.GetString(1);

                }
                catch (InvalidCastException)
                {
                    throw new Exception("Failed to fetch from database");
                }
            }
            else
            {
                throw new Exception("Failed to fetch from database");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("Failed to fetch file path");
            }
            else if (string.IsNullOrWhiteSpace(uuid))
            {
                throw new Exception("Online services not activated yet");
            }
        }

        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        return (uuid, File.ReadAllText(Path.Combine(appDataPath, filePath)));
    }

    public async Task UpdateRecipeInformation(string hash)
    {
        string sql = @"UPDATE recipes
                        SET is_modified = 1,
                            is_published = 1,
                            last_published_hash = $hash
                        WHERE hash = $hash;";
        Dictionary<string, object> parameters = new() {
            { "$hash", hash }
        };

        await databaseService.NonQueryAsync(sql, parameters);
    }

    public async Task UploadRecipe(string hash, UploadType uploadType)
    {
        (string uuid, string xmlContent) = await GetXmlFile(hash);

        HttpMethod httpMethod = uploadType == UploadType.UPLOAD ? HttpMethod.Post : HttpMethod.Put;
        string url = "recipes";
        if (httpMethod == HttpMethod.Put) url += $"/{hash}";

        HttpRequestMessage request = new(httpMethod, url)
        {
            Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml")
        };
        request.Headers.Add("uuid", uuid);

        #region API-Request
        try
        {
            HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
            throw new Exception("API unreachable");
        }
        #endregion

        await UpdateRecipeInformation(hash);
    }
}