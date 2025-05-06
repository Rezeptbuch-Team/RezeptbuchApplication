using ApplicationCore.Common.Types;
using ApplicationCore.Model;
using ApplicationCore.Interfaces;
using Moq;
using NUnit.Framework.Internal;
using System.Data.Common;
using System.Data;

namespace ApplicationCore.Tests;

[TestFixture]
public class LocalRecipeListServiceTests
{
    [Test]
    public async Task GetLocalRecipeList_ShouldCorrectlyCreateSqlAndParameters()
    {
        Filter filter = new(
            OrderBy.TITLE,
            Order.ASCENDING,
            [],
            [],
            10,
            0
        );
        string expectedSql = "SELECT * FROM recipes ORDER BY title ASC LIMIT @limit OFFSET @offset";

        // create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("hash",  typeof(string));
        table.Columns.Add("title", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("image_path", typeof(string));
        table.Columns.Add("categories", typeof(List<string>));
        table.Columns.Add("cooking_time", typeof(int));
        table.Rows.Add("h1", "recipe1", "description1", "image_path1", new List<string> { "category1" }, 30);
        DbDataReader fakeReader = table.CreateDataReader();

        // mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => s == expectedSql),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$limit") && p["$limit"].Equals(filter.count) &&
                p.ContainsKey("$offset") && p["$offset"].Equals(filter.offset)
            )
        )).ReturnsAsync(fakeReader).Verifiable();

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<RecipeEntry> result = await localRecipeListService.GetLocalRecipeList(filter);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetLocalRecipeList_ShouldReturnCorrectData()
    {
        Filter filter = new(
            OrderBy.TITLE,
            Order.ASCENDING,
            ["category2"],
            [],
            10,
            0
        );

        // create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("hash",  typeof(string));
        table.Columns.Add("title", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("image_path", typeof(string));
        table.Columns.Add("categories", typeof(List<string>));
        table.Columns.Add("cooking_time", typeof(int));
        table.Rows.Add("h3", "recipe3", "description3", "image_path3", new List<string> { "category1", "category2" }, 60);
        table.Rows.Add("h4", "recipe4", "description4", "image_path4", new List<string> { "category2" }, 15);
        table.Rows.Add("h6", "recipe6", "description6", "image_path6", new List<string> { "category2" }, 25);
        table.Rows.Add("h8", "recipe8", "description8", "image_path8", new List<string> { "category2" }, 50);
        table.Rows.Add("h10", "recipe10", "description10", "image_path10", new List<string> { "category2" }, 40);
        table.Rows.Add("h12", "recipe12", "description12", "image_path12", new List<string> { "category2" }, 45);
        DbDataReader fakeReader = table.CreateDataReader();

        // mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, object>>()
        )).ReturnsAsync(fakeReader);

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<RecipeEntry> result = await localRecipeListService.GetLocalRecipeList(filter);

        // check that the result is correct
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(6));
            Assert.That(result[0].hash, Is.EqualTo("h3"));
            Assert.That(result[0].title, Is.EqualTo("recipe3"));
            Assert.That(result[0].description, Is.EqualTo("description3"));
            Assert.That(result[0].imagePath, Is.EqualTo("image_path3"));
            Assert.That(result[0].categories, Is.EquivalentTo(new List<string> { "category1", "category2" }));
            Assert.That(result[0].cookingTime, Is.EqualTo(60));
            Assert.That(result[1].hash, Is.EqualTo("h4"));
            Assert.That(result[2].hash, Is.EqualTo("h6"));
            Assert.That(result[3].hash, Is.EqualTo("h8"));
            Assert.That(result[4].hash, Is.EqualTo("h10"));
            Assert.That(result[5].hash, Is.EqualTo("h12"));
        });

    }
}