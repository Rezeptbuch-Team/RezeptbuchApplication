using System.Data.Common;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class GetLocalRecipeService(IDatabaseService databaseService, IOnlineIdentificationService onlineIdentificationService) : IGetLocalRecipeService
{
    public async Task<Recipe> GetRecipeFromFile(string filePath)
    {
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
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

            using (XmlReader reader = XmlReader.Create(filePath, settings))
            {
                while (await reader.ReadAsync())
                {
                    // Just go through the document
                }
            }
        }
        #endregion

        #region deserialize recipe xml
        XDocument doc = XDocument.Parse(await File.ReadAllTextAsync(filePath));
        // because the xml has been validated there is no need to check for null values
        XElement root = doc.Element("recipe")!;
        Recipe recipe = new()
        {
            Hash = (string)root.Element("hash")!,
            Title = (string)root.Element("title")!,
            ImagePath = (string)root.Element("imageName")!,
            Description = (string)root.Element("description")!,
            Servings = (int)root.Element("servings")!,
            CookingTime = (int)root.Element("cookingTime")!,
            Categories = root.Element("categories")!
                              .Elements("category")
                              .Select(content => (string)content)
                              .ToList()
        };
        #region extract instructions and ingredients from xml
        foreach (XElement instructionElement in root.Element("instructions")!.Elements())
        {
            List<object> instructionItems = [];
            foreach (XNode node in instructionElement.Nodes())
            {
                switch (node)
                {
                    case XText textNode:
                        if (!string.IsNullOrWhiteSpace(textNode.Value))
                        {
                            instructionItems.Add(string.Join(" ", textNode.Value.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)));
                        }
                        break;
                    case XElement elemNode when elemNode.Name == "ingredient":
                        instructionItems.Add(new Ingredient
                        {
                            Name = (string)elemNode.Attribute("name")!,
                            Amount = (int)elemNode.Attribute("amount")!,
                            Unit = (string)elemNode.Attribute("unit")!
                        });
                        break;
                }
            }
            recipe.Instructions.Add(new Instruction
            {
                Items = instructionItems
            });
        }
        #endregion
        #endregion

        return recipe;
    }

    public async Task<Recipe> GetRecipe(string hash)
    {
        #region request filepath and publishinformation from database
        string sql = @"SELECT file_path, is_download, is_published, is_modified
                        FROM recipes
                        WHERE hash = $hash;";

        Dictionary<string, object> parameters = new() {
            { "$hash", hash }
        };
        string filePath;
        PublishOption publishOption;
        string? uuid = await onlineIdentificationService.GetUUID();
        await using (DbDataReader resultReader = await databaseService.QueryAsync(sql, parameters))
        {
            if (await resultReader.ReadAsync())
            {
                try
                {
                    filePath = resultReader.GetString(0);
                    bool is_download = resultReader.GetInt32(1) > 0;
                    bool is_published = resultReader.GetInt32(2) > 0;
                    bool is_modified = resultReader.GetInt32(3) > 0;

                    if (is_download || uuid == null) publishOption = PublishOption.FORBIDDEN;
                    else if (!is_published) publishOption = PublishOption.NOT_PUBLISHED;
                    else if (is_published && is_modified) publishOption = PublishOption.OUTDATED;
                    else publishOption = PublishOption.PUBLISHED;
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

        Recipe recipe = await GetRecipeFromFile(filePath);
        recipe.PublishOption = publishOption;

        return recipe;
    }
    
    static void ValidationCallback(object? sender, ValidationEventArgs? args)
    {
        throw new Exception("Recipe XML-file does not fit schema");
    }
}