using System.Data;
using System.Data.Common;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using ApplicationCore.Model;
using Moq;

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

        string exampleRecipePath = Path.Combine(AppContext.BaseDirectory, "Ressources", "exampleRecipe.xml");

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

        #endregion

        throw new NotImplementedException();
    }
}