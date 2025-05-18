using System.Data;
using System.Data.Common;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using ApplicationCore.Model;
using Moq;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;

namespace ApplicationCore.Tests;

[TestFixture]
public class GetLocalRecipeServiceTests
{
    /// <summary>
    /// Remve extra whitespaces and new-lines from the SQL string
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    private static string NormalizeSql(string sql)
    {
        return string.Join(" ", sql.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries));
    }

    [Test]
    public async Task WillCorrectlyRequestFromDatabase()
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        string hash = "hjasdf";
        string filePath = "hjasdf.xml";

        string exampleRecipePath = Path.Combine(AppContext.BaseDirectory, "Ressources", "exampleRecipe.xml");
        string absoluteFilePath = Path.Combine(appDataPath, filePath);
        if (File.Exists(absoluteFilePath)) File.Delete(absoluteFilePath);
        File.Copy(exampleRecipePath, absoluteFilePath);

        #region database mock
        string expectedSql = @"SELECT file_path
                                FROM recipes
                                WHERE hash = $hash;";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("file_path", typeof(string));
        table.Rows.Add("hjasdf.xml");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$hash") && p["$hash"].Equals(hash)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        // create the service and call the method
        GetLocalRecipeService service = new(mockDatabaseService.Object);
        Recipe resultRecipe = await service.GetRecipe(hash);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task WillThrowError_IfXmlDoesNotFitSchema()
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        string hash = "exampleRecipe";
        string filePath = "exampleRecipe.xml";

        string exampleRecipePath = Path.Combine(AppContext.BaseDirectory, "Ressources", "wrongExampleRecipe.xml");

        #region put example xml-file in appdata folder
        string absoluteFilePath = Path.Combine(appDataPath, filePath);
        if (File.Exists(absoluteFilePath)) File.Delete(absoluteFilePath);
        File.Copy(exampleRecipePath, absoluteFilePath);
        #endregion

        #region mock database
        string expectedSql = @"SELECT file_path
                                FROM recipes
                                WHERE hash = $hash;";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("file_path", typeof(string));
        table.Rows.Add(filePath);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$hash") && p["$hash"].Equals(hash)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        GetLocalRecipeService service = new(mockDatabaseService.Object);

        Assert.That(
            async () => await service.GetRecipe(hash),
            Throws.Exception
                .TypeOf<Exception>()
                .With.Message.EqualTo("Recipe XML-file does not fit schema")
        );

        if (File.Exists(absoluteFilePath)) File.Delete(absoluteFilePath);
    }

    [Test]
    public async Task WillNotThrowError_IfXmlFitsSchema()
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        string hash = "exampleRecipe";
        string filePath = "exampleRecipe.xml";

        string exampleRecipePath = Path.Combine(AppContext.BaseDirectory, "Ressources", filePath);

        #region put example xml-file in appdata folder
        string absoluteFilePath = Path.Combine(appDataPath, filePath);
        if (File.Exists(absoluteFilePath)) File.Delete(absoluteFilePath);
        File.Copy(exampleRecipePath, absoluteFilePath);
        #endregion

        #region mock database
        string expectedSql = @"SELECT file_path
                                FROM recipes
                                WHERE hash = $hash;";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("file_path", typeof(string));
        table.Rows.Add(filePath);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$hash") && p["$hash"].Equals(hash)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        GetLocalRecipeService service = new(mockDatabaseService.Object);

        Assert.DoesNotThrowAsync(async () => await service.GetRecipe(hash));

        if (File.Exists(absoluteFilePath)) File.Delete(absoluteFilePath);
    }

    [Test]
    public async Task WillCorrectlyReturnRecipe()
    {
        #region expected Recipe class (fitting exampleRecipe.xml)
        List<Instruction> instructions = [
            new Instruction(){
                Items = [
                    "Boil",
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    "in a large pot. Add",
                    new Ingredient{
                        Name="pasta", Amount=200, Unit="g"
                    },
                    "and cook until al dente."
                ]
            },
            new Instruction(){
                Items = [
                    "Serve."
                ]
            }
        ];
        Recipe expectedRecipe = new()
        {
            Hash = "asd",
            Title = "Pasta",
            ImagePath = "pasta.png",
            Description = "Simple pasta recipe.",
            Servings = 2,
            CookingTime = 20,
            Categories = ["Pasta", "Vegan"],
            Instructions = instructions
        };
        #endregion

        #region create filepaths
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        string hash = "asd";
        string filePath = "simpleExampleRecipe.xml";

        string exampleRecipePath = Path.Combine(AppContext.BaseDirectory, "Ressources", filePath);
        #endregion

        #region put example xml-file in appdata folder
        string absoluteFilePath = Path.Combine(appDataPath, filePath);
        if (File.Exists(absoluteFilePath)) File.Delete(absoluteFilePath);
        File.Copy(exampleRecipePath, absoluteFilePath);
        #endregion
        #region mock database
        string expectedSql = @"SELECT file_path
                                FROM recipes
                                WHERE hash = $hash;";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("file_path", typeof(string));
        table.Rows.Add(filePath);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$hash") && p["$hash"].Equals(hash)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        GetLocalRecipeService service = new(mockDatabaseService.Object);

        Recipe returnedRecipe = await service.GetRecipe(hash);

        Assert.Multiple(() =>
        {
            Assert.That(returnedRecipe.Hash, Is.EqualTo(expectedRecipe.Hash));
            Assert.That(returnedRecipe.Title, Is.EqualTo(expectedRecipe.Title));
            Assert.That(returnedRecipe.ImagePath, Is.EqualTo(expectedRecipe.ImagePath));
            Assert.That(returnedRecipe.Description, Is.EqualTo(expectedRecipe.Description));
            Assert.That(returnedRecipe.Servings, Is.EqualTo(expectedRecipe.Servings));
            Assert.That(returnedRecipe.Categories, Is.EqualTo(expectedRecipe.Categories));
            for (int i = 0; i < returnedRecipe.Instructions.Count; i++)
            {
                Assert.That(returnedRecipe.Instructions[i].Items, Is.EqualTo(expectedRecipe.Instructions[i].Items));
            }
        });
    }
}