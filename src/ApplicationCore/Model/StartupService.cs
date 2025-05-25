using System.Data.Common;
using System.Xml.Linq;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class StartupService(IDatabaseService databaseService, IGetRecipeFromFileService getRecipeFromFileService)
{
    public static void CreateAppDataFolder()
    {
        string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
    }

    private async Task UpdateDatabaseWithModifiedHash(string hash, string updatedHash)
    {
        string sql = @"UPDATE recipes
                        SET hash = $new_hash, is_modified = 1
                        WHERE hash = $old_hash;";
        Dictionary<string, object> parameters = new(){
            { "$new_hash", updatedHash },
            { "$old_hash", hash }
        };
        await databaseService.NonQueryAsync(sql, parameters);
    }

    private async Task UpdateDatabaseWithModifiedFilePath(string hash, string filePath)
    {
        string sql = @"UPDATE recipes
                        SET file_path = $file_path
                        WHERE hash = $hash;";
        Dictionary<string, object> parameters = new(){
            { "$hash", hash },
            { "$file_path", filePath }
        };
        await databaseService.NonQueryAsync(sql, parameters);
    }

    private async Task CheckForConflictInFile(string filePath, string databaseHash) {
        Recipe recipe = await getRecipeFromFileService.GetRecipeFromFile(filePath);
        string calculatedHash = recipe.CalculateHash();
        // if hash of recipe does not fit entry in database
        if (calculatedHash != databaseHash)
        {
            await UpdateDatabaseWithModifiedHash(databaseHash, calculatedHash);

            #region update <hash> in file
            // doc.Root and hashElem will not be null, because that was just checked by the GetRecipeFromFileService
            XDocument doc = XDocument.Load(filePath);
            XElement hashElem = doc.Root!.Element("hash")!;
            hashElem.Value = calculatedHash;
            doc.Save(filePath);
            #endregion
        }
    }

    private static string? FindFileWithHash(string hash)
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        foreach (string filePath in Directory.EnumerateFiles(appDataPath, "*.xml", SearchOption.AllDirectories))
        {
            XDocument doc = XDocument.Load(filePath);
            XElement? hashElem = doc.Root?.Element("hash");
            if (hashElem != null && hashElem.Value == hash)
            {
                return filePath;
            }
        }
        return null;
    }

    private async Task DeleteEntryFromDatabase(string hash)
    {
        string sql = @"DELETE FROM recipes
                        WHERE hash = $hash;";
        Dictionary<string, object> parameters = new()
        {
            { "$hash", hash }
        };

        await databaseService.NonQueryAsync(sql, parameters);
    }

    public async Task CheckForConflicts()
    {
        #region get all recipe file paths, hash
        string sql = "SELECT file_path, hash FROM recipes;";
        Dictionary<string, string> filePaths = [];

        await using (DbDataReader resultReader = await databaseService.QueryAsync(sql))
        {
            string? filePath;
            string? hash;
            if (await resultReader.ReadAsync())
            {
                filePath = resultReader.GetValue(0) as string;
                hash = resultReader.GetValue(1) as string;
                if (!string.IsNullOrWhiteSpace(filePath) && !string.IsNullOrWhiteSpace(hash))
                {
                    filePaths.Add(filePath, hash);
                }
            }
        }
        #endregion

        foreach (KeyValuePair<string, string> recipeEntry in filePaths)
        {
            if (File.Exists(recipeEntry.Key))
            {
                await CheckForConflictInFile(recipeEntry.Key, recipeEntry.Value);
            }
            else
            {
                string? foundFile = FindFileWithHash(recipeEntry.Value);
                if (foundFile != null)
                {
                    await UpdateDatabaseWithModifiedFilePath(recipeEntry.Value, foundFile);

                    await CheckForConflictInFile(foundFile, recipeEntry.Value);
                }
                else
                {
                    await DeleteEntryFromDatabase(recipeEntry.Value);
                }
            }
        }
    }

    public static void CheckForOrphanedFiles()
    {
        throw new NotImplementedException();
    }

    public static void CheckForPublishableUpdates()
    {
        throw new NotImplementedException();
    }
}