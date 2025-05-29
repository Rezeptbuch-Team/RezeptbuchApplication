using System.Data.Common;
using ApplicationCore.Model;
using ApplicationCore.Tests.Helpers;

namespace ApplicationCore.Tests.Tests;

[TestFixture]
// The Setup() method is used because the [NonParallelizable] attribute does not work correctly.
// With normal [SetUp] and [TearDown] methods, the tests are run in parallel, which causes issues with file access.
// The solution is to use a different database file for each test.
public class SqliteServiceTests
{
    [SetUp]
    public void StandardSetup()
    {
        StartupService.CreateAppDataFolder(FileHelper.GetAppDataPath());
    }

    private static SqliteService Setup(string dbDir)
    {
        if (!Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }
        string dbPath = Path.Combine(dbDir, "database.sqlite");
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
        return new SqliteService(dbDir);
    }

    [Test]
    public async Task Initialize_ShouldCreateDatabaseFile()
    {
        string dbDir = Path.Combine(AppContext.BaseDirectory, "db1");
        SqliteService sqliteService = Setup(dbDir);

        await sqliteService.InitializeAsync();

        Assert.That(File.Exists(Path.Combine(dbDir, "database.sqlite")), Is.True);
    }

    [Test]
    public async Task Initialize_ShouldAllowSubsequentOperations()
    {
        string dbDir = Path.Combine(AppContext.BaseDirectory, "db2");
        SqliteService sqliteService = Setup(dbDir);

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
