using System.Data.Common;
using System.Security.Cryptography;
using ApplicationCore.Model;

namespace ApplicationCore.Tests;

[TestFixture]
// The Setup() method is used because the [NonParallelizable] attribute does not work correctly.
// With normal [SetUp] and [TearDown] methods, the tests are run in parallel, which causes issues with file access.
// The solution is to use a different database file for each test.
public class SqliteServiceTests
{
    [SetUp]
    public void StandardSetup()
    {
        StartupService.CreateAppDataFolder();
    }

    private static SqliteService Setup(string dbPath)
    {
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
        return new SqliteService(dbPath);
    }

    [Test]
    public async Task Initialize_ShouldCreateDatabaseFile()
    {
        string dbPath = "test.sqlite";
        SqliteService sqliteService = Setup(dbPath);

        await sqliteService.InitializeAsync();

        Assert.That(File.Exists(dbPath), Is.True);
    }

    [Test]
    public async Task Initialize_ShouldAllowSubsequentOperations()
    {
        string dbPath = "test2.sqlite";
        SqliteService sqliteService = Setup(dbPath);

        await sqliteService.InitializeAsync();

        Assert.DoesNotThrowAsync(async () =>
        {
            await using (DbDataReader reader = await sqliteService.QueryAsync("SELECT 1"))
            {
                while (await reader.ReadAsync())
                {
                    // Do nothing, just read the data
                }
            }
        });
    }
}
