using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class GetRecipeFromFileService(string appDataPath) : IGetRecipeFromFileService
{
    public Recipe GetRecipeFromFile(string filePath)
    {
        #region check that recipe xml-file fits schema
        filePath = Path.Combine(appDataPath, filePath);

        var asm = typeof(SqliteService).Assembly;
        using var xsdStream = asm.GetManifestResourceStream("ApplicationCore.Schemata.recipeXml.xsd");
        using var xsdReader = XmlReader.Create(xsdStream!);

        XmlReaderSettings settings = new()
        {
            Async = true,
            ValidationType = ValidationType.Schema
        };
        settings.Schemas.Add(null, xsdReader);
        settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        using XmlReader reader = XmlReader.Create(fs, settings);
        while (reader.Read())
        {
            // process nodes…
        }
        #endregion

        #region deserialize recipe xml
        var xmlContent = File.ReadAllText(filePath);
        XDocument doc = XDocument.Parse(xmlContent);

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
    
    static void ValidationCallback(object? sender, ValidationEventArgs? args)
    {
        throw new Exception("Recipe XML-file does not fit schema");
    }
}
