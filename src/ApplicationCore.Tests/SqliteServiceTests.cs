using System.Security.Cryptography;
using ApplicationCore.Model;

namespace ApplicationCore.Tests;

[TestFixture]
// The Setup() and TearDown() methods are used because the [NonParallelizable] attribute does not work correctly.
// With normal [SetUp] and [TearDown] methods, the tests are run in parallel, which causes issues with file access.
// The solution is to use a different database file for each test.
public class SqliteServiceTests
{
    private static SqliteService Setup(string dbPath)
    {
        return new SqliteService(dbPath);
    }

    private static void TearDown(string dbPath)
    {
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }

    [Test]
    public async Task Initialize_ShouldCreateDatabaseFile()
    {
        string dbPath = "test.sqlite";
        SqliteService sqliteService = Setup(dbPath);

        await sqliteService.InitializeAsync();

        Assert.That(File.Exists(dbPath), Is.True);

        TearDown(dbPath);
    }

    [Test]
    public async Task Initialize_ShouldAllowSubsequentOperations()
    {
        string dbPath = "test2.sqlite";
        SqliteService sqliteService = Setup(dbPath);

        await sqliteService.InitializeAsync();

        Assert.DoesNotThrowAsync(async () =>
        {
            await sqliteService.QueryAsync("SELECT 1");
        });

        TearDown(dbPath);
    }
}
