using System.Data.Common;
using System.Xml;
using System.Xml.Schema;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class GetLocalRecipeService(IDatabaseService databaseService) : IGetLocalRecipeService
{
    public async Task<Recipe> GetRecipe(string hash)
    {
        #region request filepath from database
        string sql = @"SELECT file_path
                        FROM recipes
                        WHERE hash = $hash;";

        Dictionary<string, object> parameters = new() {
            { "$hash", hash }
        };
        string filePath;
        await using (DbDataReader resultReader = await databaseService.QueryAsync(sql, parameters))
        {
            if (await resultReader.ReadAsync())
            {
                try
                {
                    filePath = resultReader.GetString(0);
                }
                catch (InvalidCastException)
                {
                    throw new Exception("Failed to fetch file path for hash");
                }
            }
            else
            {
                throw new Exception("Recipe not found");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("Failed to fetch file path for hash");
            }
        }
        #endregion

        #region check that recipe xml-file fits schema
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        filePath = Path.Combine(appDataPath, filePath);

        string xsdPath = Path.Combine(AppContext.BaseDirectory, "Schemata", "recipeXml.xsd");
        using (FileStream stream = File.OpenRead(xsdPath))
        {
            XmlReaderSettings settings = new()
            {
                Async = true,
                ValidationType = ValidationType.Schema
            };
            settings.Schemas.Add(null, xsdPath);
            settings.ValidationEventHandler += new ValidationEventHandler(XmlError);

            using (XmlReader reader = XmlReader.Create(filePath, settings))
            {
                while (await reader.ReadAsync())
                {
                    // Just go through the document
                }
            }
        }
        #endregion

        return new Recipe();
    }
    
    static void XmlError(object? sender, ValidationEventArgs? args)
    {
        throw new Exception("Recipe XML-file does not fit schema");
    }
}