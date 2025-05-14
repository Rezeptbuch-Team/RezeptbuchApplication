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
    /// <summary>
    /// Remve extra whitespaces and new-lines from the SQL string
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    private static string NormalizeSql(string sql) {
        return string.Join(" ", sql.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries));
    }

    [Test]
    public async Task GetLocalRecipes_ShouldCorrectlyCreateSqlAndParameters()
    {
        Filter filter = new(
            OrderBy.TITLE,
            Order.ASCENDING,
            ["category1"],
            [],
            10,
            0
        );
        string expectedSql = @"SELECT DISTINCT r.hash AS hash, r.title AS title, 
                                        r.description AS description, 
                                        r.image_path AS image_path,
                                        r.cooking_time AS cooking_time,
                                        c.name AS category
                                FROM recipes r 
                                JOIN recipe_category rc ON r.hash = rc.hash 
                                JOIN categories c ON rc.category_id = c.id
                                WHERE c.name IN ($cat1)
                                ORDER BY title ASC
                                LIMIT $limit 
                                OFFSET $offset;";

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("hash",  typeof(string));
        table.Columns.Add("title", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("image_path", typeof(string));
        table.Columns.Add("cooking_time", typeof(int));
        table.Columns.Add("category", typeof(string));
        table.Rows.Add("h1", "recipe1", "description1", "image_path1", 30, "category1");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$limit") && p["$limit"].Equals(filter.count) &&
                p.ContainsKey("$offset") && p["$offset"].Equals(filter.offset) &&
                p.ContainsKey("$cat1") && p["$cat1"].Equals(filter.categories[0])
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<RecipeEntry> result = await localRecipeListService.GetLocalRecipeList(filter);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetLocalRecipes_ShouldCorrectlyCreateSqlAndParameters_DifferentFilter()
    {
        Filter filter = new(
            OrderBy.COOKINGTIME,
            Order.DESCENDING,
            ["category2", "category3"],
            ["tomato", "cheese"],
            5,
            10
        );
        string expectedSql = @"SELECT DISTINCT r.hash AS hash, r.title AS title, 
                                        r.description AS description, 
                                        r.image_path AS image_path,
                                        r.cooking_time AS cooking_time,
                                        c.name AS category
                                FROM recipes r 
                                JOIN recipe_category rc ON r.hash = rc.hash 
                                JOIN categories c ON rc.category_id = c.id
                                JOIN recipe_ingredient ri ON r.hash = ri.hash
                                JOIN ingredients i ON ri.ingredient_id = i.id
                                WHERE c.name IN ($cat1, $cat2) 
                                    AND i.name IN ($ing1, $ing2)
                                ORDER BY cooking_time DESC
                                LIMIT $limit 
                                OFFSET $offset;";

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("hash",  typeof(string));
        table.Columns.Add("title", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("image_path", typeof(string));
        table.Columns.Add("cooking_time", typeof(int));
        table.Columns.Add("category", typeof(string));
        table.Rows.Add("h1", "recipe1", "description1", "image_path1", 30,  "category2");
        table.Rows.Add("h1", "recipe1", "description1", "image_path1", 30, "category3");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$limit") && p["$limit"].Equals(filter.count) &&
                p.ContainsKey("$offset") && p["$offset"].Equals(filter.offset) &&
                p.ContainsKey("$cat1") && p["$cat1"].Equals(filter.categories[0]) &&
                p.ContainsKey("$cat2") && p["$cat2"].Equals(filter.categories[1]) &&
                p.ContainsKey("$ing1") && p["$ing1"].Equals(filter.availableIngredients[0]) &&
                p.ContainsKey("$ing2") && p["$ing2"].Equals(filter.availableIngredients[1])
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<RecipeEntry> result = await localRecipeListService.GetLocalRecipeList(filter);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetLocalRecipes_ShouldReturnCorrectData()
    {
        Filter filter = new(
            OrderBy.TITLE,
            Order.ASCENDING,
            ["category1", "category2"],
            ["tomato"],
            10,
            0
        );

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("hash",  typeof(string));
        table.Columns.Add("title", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("image_path", typeof(string));
        table.Columns.Add("cooking_time", typeof(int));
        table.Columns.Add("category", typeof(string));
        table.Rows.Add("h1", "recipe1", "description1", "image_path1", 30, "category1");
        table.Rows.Add("h1", "recipe1", "description1", "image_path1", 30,  "category2");
        table.Rows.Add("h2", "recipe2", "description2", "image_path2", 15, "category1");
        table.Rows.Add("h2", "recipe2", "description2", "image_path2", 15, "category2");
        table.Rows.Add("h4", "recipe4", "description4", "image_path4", 20,  "category2");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, object>>()
        )).ReturnsAsync(fakeReader);
        #endregion
        
        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<RecipeEntry> result = await localRecipeListService.GetLocalRecipeList(filter);

        // check that the result is correct
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].hash, Is.EqualTo("h1"));
            Assert.That(result[0].title, Is.EqualTo("recipe1"));
            Assert.That(result[0].description, Is.EqualTo("description1"));
            Assert.That(result[0].imagePath, Is.EqualTo("image_path1"));
            Assert.That(result[0].categories, Contains.Item("category1"));
            Assert.That(result[0].categories, Contains.Item("category2"));
            Assert.That(result[0].cookingTime, Is.EqualTo(30));

            Assert.That(result[1].hash, Is.EqualTo("h2"));
            Assert.That(result[1].categories, Contains.Item("category1"));
            Assert.That(result[1].categories, Contains.Item("category2"));

            Assert.That(result[2].hash, Is.EqualTo("h4"));
            Assert.That(result[2].categories, Contains.Item("category2"));
        });

    }

    [Test]
    public async Task GetCategories_ShouldCorrectlyCreateSqlAndParametersWhenOrderingByTitle()
    {
        FilterOptionOrderBy orderBy = FilterOptionOrderBy.TITLE;
        Order order = Order.ASCENDING;
        int limit = 10;
        int offset = 0;

        string expectedSql = @"SELECT c.name AS name, COUNT(rc.hash) AS count
                                FROM categories c
                                JOIN recipe_category rc ON c.id = rc.category_id
                                GROUP BY c.name
                                ORDER BY name ASC
                                LIMIT $limit 
                                OFFSET $offset;";

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("count", typeof(int));
        table.Rows.Add("category1", 5);
        table.Rows.Add("category2", 3);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$limit") && p["$limit"].Equals(limit) &&
                p.ContainsKey("$offset") && p["$offset"].Equals(offset)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<FilterOption> result = await localRecipeListService.GetCategories(orderBy, order, limit, offset);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetCategories_ShouldCorrectlyCreateSqlAndParameters()
    {
        FilterOptionOrderBy orderBy = FilterOptionOrderBy.COUNT;
        Order order = Order.DESCENDING;
        int limit = 10;
        int offset = 0;

        string expectedSql = @"SELECT c.name AS name, COUNT(rc.hash) AS count
                                FROM categories c
                                JOIN recipe_category rc ON c.id = rc.category_id
                                GROUP BY c.name
                                ORDER BY count DESC
                                LIMIT $limit 
                                OFFSET $offset;";

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("count", typeof(int));
        table.Rows.Add("category1", 5);
        table.Rows.Add("category2", 3);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$limit") && p["$limit"].Equals(limit) &&
                p.ContainsKey("$offset") && p["$offset"].Equals(offset)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<FilterOption> result = await localRecipeListService.GetCategories(orderBy, order, limit, offset);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetCategories_ShouldReturnCorrectData()
    {
        FilterOptionOrderBy orderBy = FilterOptionOrderBy.COUNT;
        Order order = Order.DESCENDING;
        int limit = 10;
        int offset = 0;

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("count", typeof(int));
        table.Rows.Add("category1", 13);
        table.Rows.Add("category5", 7);
        table.Rows.Add("category2", 2);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, object>>()
        )).ReturnsAsync(fakeReader);
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<FilterOption> result = await localRecipeListService.GetCategories(orderBy, order, limit, offset);

        // check that the result is correct
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(new FilterOption("category1", 13)));
            Assert.That(result[1], Is.EqualTo(new FilterOption("category5", 7)));
            Assert.That(result[2], Is.EqualTo(new FilterOption("category2", 2)));
        });
    }

    [Test]
    public async Task GetIngredients_ShouldCorrectlyCreateSqlAndParametersWhenOrderingByTitle()
    {
        FilterOptionOrderBy orderBy = FilterOptionOrderBy.TITLE;
        Order order = Order.ASCENDING;
        int limit = 10;
        int offset = 0;

        string expectedSql = @"SELECT i.name AS name, COUNT(ri.hash) AS count
                                FROM ingredients i
                                JOIN recipe_ingredient ri ON i.id = ri.ingredient_id
                                GROUP BY i.name
                                ORDER BY name ASC
                                LIMIT $limit 
                                OFFSET $offset;";

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("count", typeof(int));
        table.Rows.Add("tomato", 5);
        table.Rows.Add("cheese", 3);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$limit") && p["$limit"].Equals(limit) &&
                p.ContainsKey("$offset") && p["$offset"].Equals(offset)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<FilterOption> result = await localRecipeListService.GetIngredients(orderBy, order, limit, offset);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetIngredients_ShouldCorrectlyCreateSqlAndParameters()
    {
        FilterOptionOrderBy orderBy = FilterOptionOrderBy.COUNT;
        Order order = Order.DESCENDING;
        int limit = 10;
        int offset = 0;

        string expectedSql = @"SELECT i.name AS name, COUNT(ri.hash) AS count
                                FROM ingredients i
                                JOIN recipe_ingredient ri ON i.id = ri.ingredient_id
                                GROUP BY i.name
                                ORDER BY count DESC
                                LIMIT $limit 
                                OFFSET $offset;";

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("count", typeof(int));
        table.Rows.Add("tomato", 5);
        table.Rows.Add("cheese", 3);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$limit") && p["$limit"].Equals(limit) &&
                p.ContainsKey("$offset") && p["$offset"].Equals(offset)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<FilterOption> result = await localRecipeListService.GetIngredients(orderBy, order, limit, offset);

        // check that the queryAsync method was called
        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetIngredients_ShouldReturnCorrectData()
    {
        FilterOptionOrderBy orderBy = FilterOptionOrderBy.COUNT;
        Order order = Order.DESCENDING;
        int limit = 10;
        int offset = 0;

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("count", typeof(int));
        table.Rows.Add("tomato", 13);
        table.Rows.Add("cheese", 7);
        table.Rows.Add("flour", 2);
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, object>>()
        )).ReturnsAsync(fakeReader);
        #endregion

        // create the service and call the method
        LocalRecipeListService localRecipeListService = new(mockDatabaseService.Object);
        List<FilterOption> result = await localRecipeListService.GetIngredients(orderBy, order, limit, offset);

        // check that the result is correct
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(new FilterOption("tomato", 13)));
            Assert.That(result[1], Is.EqualTo(new FilterOption("cheese", 7)));
            Assert.That(result[2], Is.EqualTo(new FilterOption("flour", 2)));
        });
    }

}