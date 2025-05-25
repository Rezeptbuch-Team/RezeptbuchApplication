using System.Net;
using System.Data;
using System.Data.Common;
using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;
using ApplicationCore.Model;
using Moq;
using Moq.Protected;

namespace ApplicationCore.Tests.Tests;

[TestFixture]
public class UploadTests
{
    Mock<IDatabaseService> databaseService;
    string hash = "123asd";
    string old_hash = "yxcasd";

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
        string expectedSql = @"SELECT r.file_path, r.image_path, r.last_published_hash, (
                                    SELECT value FROM app_info WHERE key = 'uuid'
                                ) AS uuid
                                FROM recipes r
                                WHERE r.hash = $hash;";
        #region create a fake DataTable to simulate the database response
        DataTable table = new();
        table.Columns.Add("file_path", typeof(string));
        table.Columns.Add("image_path", typeof(string));
        table.Columns.Add("last_published_hash", typeof(string));
        table.Columns.Add("uuid", typeof(string));
        table.Rows.Add(exampleRecipePath, "someImagePath", old_hash, "someUUID");
        DbDataReader fakeReader = table.CreateDataReader();
        #endregion

        #region mock database controller
        databaseService = new();
        databaseService.Setup(db => db.QueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => SqlHelper.NormalizeSql(s) == SqlHelper.NormalizeSql(expectedSql)),
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
        (string returnedUuid, string? imagePath, string last_published_hash, string returnedXml) = await service.GetXmlFile(hash);

        Assert.Multiple(() =>
        {
            Assert.That(returnedXml, Is.EqualTo("testcontent"));
            Assert.That(returnedUuid, Is.EqualTo("someUUID"));
            Assert.That(last_published_hash, Is.EqualTo(old_hash));
            Assert.That(imagePath, Is.EqualTo("someImagePath"));
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
                && req.RequestUri == new Uri("https://api.server.com/recipes")
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
                && req.RequestUri == new Uri($"https://api.server.com/recipes/{old_hash}")
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

    [Test]
    public async Task UpdateRecipeInformation_ShouldExecuteTheCorrectSql()
    {
        #region Arrange
        string expectedSql = @"UPDATE recipes 
                                SET is_modified = 1, is_published = 1, last_published_hash = $hash
                                WHERE hash = $hash;";

        var mockDatabaseService = new Mock<IDatabaseService>();
        mockDatabaseService.Setup(db => db.NonQueryAsync(
            // check that the SQL query is correct
            It.Is<string>(s => SqlHelper.NormalizeSql(s) == SqlHelper.NormalizeSql(expectedSql)),
            // check that the parameters are correct
            It.Is<IDictionary<string, object>>(p =>
               p.ContainsKey("$hash") && p["$hash"].Equals(hash)
            )
        )).ReturnsAsync(1).Verifiable();

        // not used in this test
        HttpClient httpClient = new();
        #endregion

        UploadService service = new(mockDatabaseService.Object, httpClient);
        await service.UpdateRecipeInformation(hash);

        mockDatabaseService.Verify();
    }

    [Test]
    public async Task UploadImage_PostsImageWithUUID()
    {
        string exampleImagePath = Path.Combine(AppContext.BaseDirectory, "Ressources", "test.png");
        byte[] capturedBytes = [];
        string capturedUuid = "";

        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post
                && req.RequestUri == new Uri($"https://api.server.com/images/{hash}")
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
                capturedBytes = await req.Content.ReadAsByteArrayAsync();
            }
        })
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };


        UploadService service = new(databaseService.Object, httpClient);
        await service.UploadImage(hash, exampleImagePath, "someUUID", HttpMethod.Post);

        Assert.Multiple(() =>
        {
            Assert.That(capturedUuid, Is.EqualTo("someUUID"));
            Assert.That(capturedBytes, Is.EqualTo(File.ReadAllBytes(exampleImagePath)));
        });
    }

    [Test]
    public async Task UploadImage_PutsImageWithUUID()
    {
        string exampleImagePath = Path.Combine(AppContext.BaseDirectory, "Ressources", "test.png");
        byte[] capturedBytes = [];
        string capturedUuid = "";

        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put
                && req.RequestUri == new Uri($"https://api.server.com/images/{hash}")
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
                capturedBytes = await req.Content.ReadAsByteArrayAsync();
            }
        })
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };


        UploadService service = new(databaseService.Object, httpClient);
        await service.UploadImage(hash, exampleImagePath, "someUUID", HttpMethod.Put);

        Assert.Multiple(() =>
        {
            Assert.That(capturedUuid, Is.EqualTo("someUUID"));
            Assert.That(capturedBytes, Is.EqualTo(File.ReadAllBytes(exampleImagePath)));
        });
    }
}