using System.Data;
using System.Data.Common;
using ApplicationCore.Interfaces;
using ApplicationCore.Model;
using Moq;

namespace ApplicationCore.Tests.Tests;

[TestFixture]
public class OnlineIdentificationServiceTests
{
    [Test]
    public async Task GetUUID_ShouldCorrectlyFormSql()
    {
        #region database mock
        string expectedSql = @"SELECT value
                                FROM app_info
                                WHERE key = 'uuid';";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("value", typeof(string));
        table.Rows.Add("asd123");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.Is<string>(s => SqlHelper.NormalizeSql(s) == SqlHelper.NormalizeSql(expectedSql)),
            It.IsAny<IDictionary<string, object>?>()
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        OnlineIdentificationService service = new(mockDatabaseService.Object);
        string? uuid = await service.GetUUID();

        mockDatabaseService.Verify();
    }

    [Test]
    public async Task GetUUID_ShouldReturnTrueAndUUID_IfExists()
    {
        #region database mock
        string expectedSql = @"SELECT value
                                FROM app_info
                                WHERE key = 'uuid';";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("value", typeof(string));
        table.Rows.Add("asd123");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.Is<string>(s => SqlHelper.NormalizeSql(s) == SqlHelper.NormalizeSql(expectedSql)),
            It.IsAny<IDictionary<string, object>?>()
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        OnlineIdentificationService service = new(mockDatabaseService.Object);
        string? uuid = await service.GetUUID();

        Assert.Multiple(() =>
        {
            Assert.That(uuid, Is.EqualTo("asd123"));
        });
    }

    [Test]
    public async Task GetUUID_ShouldReturnFalse_IfNotExists()
    {
        #region database mock
        string expectedSql = @"SELECT value
                                FROM app_info
                                WHERE key = 'uuid';";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("value", typeof(string));
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.Is<string>(s => SqlHelper.NormalizeSql(s) == SqlHelper.NormalizeSql(expectedSql)),
            It.IsAny<IDictionary<string, object>?>()
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        OnlineIdentificationService service = new(mockDatabaseService.Object);
        string? uuid = await service.GetUUID();

        Assert.Multiple(() =>
        {
            Assert.That(uuid, Is.Null);
        });
    }

    [Test]
    public async Task CreateUUID_ShouldReturnUUID_IfItExists()
    {
        #region database mock
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("value", typeof(string));
        table.Rows.Add("123asd");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, object>?>()
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion

        OnlineIdentificationService service = new(mockDatabaseService.Object);
        string? uuid = await service.CreateUUID();

        Assert.That(uuid, Is.EqualTo("123asd"));
    }

    [Test]
    public async Task CreateUUID_ShouldCreateAndReturnUUID_IfItDoesNotExist()
    {
        #region database mock
        string expectedInsertSql = @"INSERT INTO app_info (key, value) VALUES ($key, $value);";

        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("value", typeof(string));
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.QueryAsync(
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, object>?>()
        )).ReturnsAsync(fakeReader).Verifiable();
        mockDatabaseService.Setup(db => db.NonQueryAsync(
            It.Is<string>(s => SqlHelper.NormalizeSql(s) == SqlHelper.NormalizeSql(expectedInsertSql)),
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$key") && p["$key"].Equals("uuid") &&
                p.ContainsKey("$value")
            )
        )).ReturnsAsync(1).Verifiable();
        #endregion
        #endregion

        OnlineIdentificationService service = new(mockDatabaseService.Object);
        string? uuid = await service.CreateUUID();

        Assert.That(uuid, Has.Length.GreaterThan(4));
        mockDatabaseService.Verify();
    }
}