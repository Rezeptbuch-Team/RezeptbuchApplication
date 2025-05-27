using System.Data.Common;
using System.Net.Http.Headers;
using System.Text;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class UploadService(IDatabaseService databaseService, HttpClient httpClient)
{
    public async Task<(string uuid, string? imagePath, string last_published_hash, string xmlContent)> GetXmlFile(string hash)
    {
        string sql = @"SELECT r.file_path, r.image_path, r.last_published_hash, (
                            SELECT value FROM app_info WHERE key = 'uuid'
                        ) AS uuid
                        FROM recipes r
                        WHERE r.hash = $hash;";
        Dictionary<string, object> parameters = new()
        {
            ["$hash"] = hash
        };

        string filePath = "";
        string? imagePath = "";
        string? last_published_hash = "";
        string? uuid = "";
        await using (DbDataReader reader = await databaseService.QueryAsync(sql, parameters))
        {
            if (await reader.ReadAsync())
            {
                try
                {
                    filePath = reader.GetString(0);
                    imagePath = reader.GetValue(1) as string;
                    last_published_hash = reader.GetValue(2) as string;
                    uuid = reader.GetValue(3) as string;

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
            else if (string.IsNullOrWhiteSpace(last_published_hash))
            {
                throw new Exception("Last published hash missing");
            }
        }

        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        return (uuid, imagePath, last_published_hash, File.ReadAllText(Path.Combine(appDataPath, filePath)));
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

    public async Task UploadImage(string hash, string imagePath, string uuid, HttpMethod httpMethod)
    {
        string url = $"images/{hash}";

        if (!File.Exists(imagePath)) return;

        using ByteArrayContent content = new(await File.ReadAllBytesAsync(imagePath));
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        HttpRequestMessage request = new(httpMethod, url)
        {
            Content = content
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
    }

    public async Task UploadRecipe(string hash, UploadType uploadType)
    {
        (string uuid, string? imagePath, string last_published_hash, string xmlContent) = await GetXmlFile(hash);

        HttpMethod httpMethod = uploadType == UploadType.UPLOAD ? HttpMethod.Post : HttpMethod.Put;
        string url = "recipes";
        if (httpMethod == HttpMethod.Put) url += $"/{last_published_hash}";

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

        // upload/update image
        if (!string.IsNullOrWhiteSpace(imagePath))
        {
            await UploadImage(hash, imagePath, uuid, httpMethod);
        }

        await UpdateRecipeInformation(hash);
    }
}