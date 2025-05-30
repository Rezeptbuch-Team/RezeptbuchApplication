using System.Data.Common;
using System.Xml.Linq;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class StartupService(IDatabaseService databaseService, IGetRecipeFromFileService getRecipeFromFileService, IDownloadRecipeService downloadRecipeService, string appDataPath)
{
    #region Helper functions
    private async Task UpdateDatabaseWithModifiedHash(string hash, string updatedHash)
    {
        string sql = @"UPDATE recipes
                        SET hash = $new_hash, is_modified = 1
                        WHERE hash = $old_hash;
                        UPDATE recipe_category
                        SET hash = $new_hash
                        WHERE hash = $old_hash;
                        UPDATE recipe_ingredient
                        SET hash = $new_hash
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

    private async Task CheckForConflictInFile(string filePath, string databaseHash)
    {
        try
        {
            Recipe recipe = getRecipeFromFileService.GetRecipeFromFile(filePath);
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
        catch
        {
            // we do not care if there are errors during this
        }
    }

    private IEnumerable<string> XmlFilesInDirectory()
    {
        foreach (string filePath in Directory.EnumerateFiles(appDataPath, "*.xml", SearchOption.AllDirectories))
        {
            yield return filePath;
        }
    }

    private string? FindFileWithHash(string hash)
    {
        foreach (string filePath in XmlFilesInDirectory())
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
    #endregion

    public static void CreateAppDataFolder(string appDataPath)
    {
        if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
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
            while (await resultReader.ReadAsync())
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
                await DeleteEntryFromDatabase(recipeEntry.Value);
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

    public async Task CheckForOrphanedFiles()
    {
        foreach (string filePath in XmlFilesInDirectory())
        {
            try
            {
                Recipe recipe = getRecipeFromFileService.GetRecipeFromFile(filePath);
                if (!await downloadRecipeService.IsHashInDatabase(recipe.Hash))
                {
                    await downloadRecipeService.AddRecipeToDatabase(recipe, filePath);
                }
            }
            catch
            {
                // probably means that there is an error loading the recipe which we do not care about
            }
        }
    }
}