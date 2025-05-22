using System.Net;
using System.Data;
using System.Data.Common;
using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;
using ApplicationCore.Model;
using Moq;
using Moq.Protected;

namespace ApplicationCore.Tests;

[TestFixture]
public class UploadTests
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

    Mock<IDatabaseService> databaseService;
    string hash = "123asd";

    [SetUp]
    public void Setup()
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        string filePath = "someFile.xml";
        string absoluteFilePath = Path.Combine(appDataPath, filePath);
        if (File.Exists(absoluteFilePath)) File.Delete(absoluteFilePath);
        string exampleRecipePath = Path.Combine(AppContext.BaseDirectory, "Ressources", "testToSeeIfBeingRead.xml");
        File.Copy(exampleRecipePath, absoluteFilePath);

        #region database mock
        string expectedSql = @"SELECT r.file_path, (
                                    SELECT value FROM app_info WHERE key = 'uuid'
                                ) AS uuid
                                FROM recipes r
                                WHERE r.hash = $hash;";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("file_path", typeof(string));
        table.Columns.Add("uuid", typeof(string));
        table.Rows.Add(exampleRecipePath, "someUUID");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        databaseService = new();
        databaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => NormalizeSql(s) == NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
                p.ContainsKey("$hash") && p["$hash"].Equals(hash)
            )
        )).ReturnsAsync(fakeReader).Verifiable();
        #endregion
        #endregion
    }

    [Test]
    public async Task GetXmlFile_CorrectlyGetsXmlFile()
    {
        UploadService service = new(databaseService.Object, new HttpClient());
        (string returnedUuid, string returnedXml) = await service.GetXmlFile(hash);

        Assert.Multiple(() =>
        {
            Assert.That(returnedXml, Is.EqualTo("testcontent"));
            Assert.That(returnedUuid, Is.EqualTo("someUUID"));
        });
        databaseService.Verify();
    }

    [Test]
    public async Task UploadRecipe_PostsFileContentWithUUID()
    {
        string capturedXml = "";
        string capturedUuid = "";

        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post
                && req.RequestUri == new Uri($"https://api.server.com/recipes")
            ),
            ItExpr.IsAny<CancellationToken>()
        )
        .Callback<HttpRequestMessage, CancellationToken>(async (req, _) =>
        {
            // Capture UUID header
            if (req.Headers.TryGetValues("uuid", out var values))
            {
                capturedUuid = values.FirstOrDefault()!;
            }

            // Capture XML content
            if (req.Content != null)
            {
                capturedXml = await req.Content.ReadAsStringAsync();
            }
        })
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };


        UploadService service = new(databaseService.Object, httpClient);
        await service.UploadRecipe(hash, UploadType.UPLOAD);

        Assert.Multiple(() =>
        {
            Assert.That(capturedUuid, Is.EqualTo("someUUID"));
            Assert.That(capturedXml, Is.EqualTo("testcontent"));
        });
    }

    /// <summary>
    /// check that updating a recipe works by just changing the httpmethod
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task UploadRecipe_PostsFileContentWithUUID_AndCorrectUrl_IfMethodIsPut()
    {
        string capturedXml = "";
        string capturedUuid = "";

        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put
                && req.RequestUri == new Uri($"https://api.server.com/recipes/{hash}")
            ),
            ItExpr.IsAny<CancellationToken>()
        )
        .Callback<HttpRequestMessage, CancellationToken>(async (req, _) =>
        {
            // Capture UUID header
            if (req.Headers.TryGetValues("uuid", out var values))
            {
                capturedUuid = values.FirstOrDefault()!;
            }

            // Capture XML content
            if (req.Content != null)
            {
                capturedXml = await req.Content.ReadAsStringAsync();
            }
        })
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };


        UploadService service = new(databaseService.Object, httpClient);
        await service.UploadRecipe(hash, UploadType.UPDATE);

        Assert.Multiple(() =>
        {
            Assert.That(capturedUuid, Is.EqualTo("someUUID"));
            Assert.That(capturedXml, Is.EqualTo("testcontent"));
        });
    }
}