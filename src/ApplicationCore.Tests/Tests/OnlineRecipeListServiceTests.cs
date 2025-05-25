using ApplicationCore.Common.Types;
using ApplicationCore.Model;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;

namespace ApplicationCore.Tests;

[TestFixture]
public class OnlineRecipeListServiceTests
{
    // warning about non-nullable fields being uninitialized is disabled,
    // because the field onlineRecipeListService is initialized in the Setup method
    // which is called before each test execution
    // and ensures it is properly initialized at runtime
    // and the field is not used before that
#pragma warning disable CS8618
    OnlineRecipeListService onlineRecipeListService;
#pragma warning restore CS8618
    readonly byte[] pngBytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Ressources", "test.png"));

    private readonly string exampleJson = @"{
    ""recipes"": [{
        ""hash"": ""asdafc"",
        ""title"": ""title1"",
        ""description"": ""description1"",
        ""categories"": [""category1"", ""category2""],
        ""cooking_time"": 15
    },
    {
        ""hash"": ""agdgd"",
        ""title"": ""title2"",
        ""description"": ""description2"",
        ""categories"": [""category1"", ""category2""],
        ""cooking_time"": 30
    }]
    }";

    [SetUp]
    public void Setup()
    {
        StartupService.CreateAppDataFolder();

        HttpClient httpClient = new()
        {
            BaseAddress = new Uri("http://api.server.com/")
        };
        onlineRecipeListService = new(httpClient);
    }

    [Test]
    public void WillLeaveOutDefaults_WhenBuildingUrl()
    {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], null, 10, 0);

        string url = onlineRecipeListService.BuildListUrl(filter);

        Assert.Multiple(() =>
        {
            Assert.That(url, Does.Not.Contain("order_by=title"));
            Assert.That(url, Does.Not.Contain("order_by=cooking_time"));
            Assert.That(url, Does.Not.Contain("order=asc"));
            Assert.That(url, Does.Not.Contain("order=desc"));
        });
    }

    [Test]
    public void WillShowOrderAndOrderBy_WhenNotDefault()
    {
        Filter filter = new(OrderBy.COOKINGTIME, Order.DESCENDING, ["category1", "category2"], null, 10, 0);

        string url = onlineRecipeListService.BuildListUrl(filter);

        Assert.Multiple(() =>
        {
            Assert.That(url, Does.Not.Contain("order_by=title"));
            Assert.That(url, Does.Contain("order_by=cooking_time"));
            Assert.That(url, Does.Not.Contain("order=asc"));
            Assert.That(url, Does.Contain("order=desc"));
        });
    }

    [Test]
    public void WillListCategories_WhenBuildingUrl()
    {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], null, 10, 0);

        string url = onlineRecipeListService.BuildListUrl(filter);

        Assert.That(url, Does.Contain("categories=category1,category2"));
    }

    [Test]
    public void WillStartCorrectly_WhenBuildingUrl()
    {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], null, 10, 0);

        string url = onlineRecipeListService.BuildListUrl(filter);

        Assert.That(url, Does.StartWith("/list?"));
    }

    [Test]
    public async Task WillCorrectlyExtractRecipeEntriesFromJson()
    {
        #region Arrange
        Mock<HttpMessageHandler> mockHttpMessageHandler = new();
        mockHttpMessageHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(exampleJson)
        });

        HttpClient mockHttpClient = new(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://api.server.com/")
        };
        OnlineRecipeListService service = new(mockHttpClient);
        #endregion

        #region Act
        Filter filter = new(OrderBy.COOKINGTIME, Order.DESCENDING, ["category1", "category2"], null, 10, 0);
        List<RecipeEntry> result = await service.GetRecipeList(filter);
        #endregion

        #region Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            #region first recipe entry
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Hash, Is.EqualTo("asdafc"));
                Assert.That(result[0].Title, Is.EqualTo("title1"));
                Assert.That(result[0].Description, Is.EqualTo("description1"));
                Assert.That(result[0].CookingTime, Is.EqualTo(15));
                Assert.That(result[0].Categories, Does.Contain("category1"));
                Assert.That(result[0].Categories, Does.Contain("category2"));
            });
            #endregion
            #region second recipe entry
            Assert.Multiple(() =>
            {
                Assert.That(result[1].Hash, Is.EqualTo("agdgd"));
                Assert.That(result[1].Title, Is.EqualTo("title2"));
                Assert.That(result[1].Description, Is.EqualTo("description2"));
                Assert.That(result[1].CookingTime, Is.EqualTo(30));
                Assert.That(result[1].Categories, Does.Contain("category1"));
                Assert.That(result[1].Categories, Does.Contain("category2"));
            });
            #endregion
        });
        #endregion
    }

    [Test]
    public async Task WillTryToDownloadImages()
    {
        #region Arrange
        #region create a mock that also contains a route where an image is returned
        Mock<HttpMessageHandler> mockHttpMessageHandler = new();
        mockHttpMessageHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
        {
            #region image response
            string path = request.RequestUri?.AbsolutePath ?? "";
            if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                ByteArrayContent imageContent = new(pngBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = imageContent
                };
            }
            #endregion
            #region default data response
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(exampleJson)
            };
            #endregion
        });
        #endregion

        #region initialize the service
        HttpClient mockHttpClient = new(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };
        OnlineRecipeListService service = new(mockHttpClient);
        #endregion
        #endregion

        #region Act
        Filter filter = new(OrderBy.COOKINGTIME, Order.DESCENDING, ["category1", "category2"], null, 10, 0);
        List<RecipeEntry> result = await service.GetRecipeList(filter);
        #endregion


        #region Assert that the "/images/{hash}" route is called
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri != null &&
                req.RequestUri.AbsolutePath.StartsWith("/images/")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
        #endregion
    }

    [Test]
    public async Task WillCorrectlyDownloadImages()
    {
        #region Arrange
        #region create image paths and delete them if they exist
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        string[] expectedFilePaths = [
            Path.Combine(appDataPath, "asdafc.png"),
            Path.Combine(appDataPath, "agdgd.png")
        ];
        foreach (string path in expectedFilePaths)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        #endregion

        #region create a mock that also contains a route where an image is returned
        Mock<HttpMessageHandler> mockHttpMessageHandler = new();
        mockHttpMessageHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
        {
            #region image response
            string path = request.RequestUri?.AbsolutePath ?? "";
            if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                ByteArrayContent imageContent = new(pngBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = imageContent
                };
            }
            #endregion
            #region default data response
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(exampleJson)
            };
            #endregion
        });
        #endregion

        #region initialize the service
        HttpClient mockHttpClient = new(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };
        OnlineRecipeListService service = new(mockHttpClient);
        #endregion
        #endregion

        #region Act
        Filter filter = new(OrderBy.COOKINGTIME, Order.DESCENDING, ["category1", "category2"], null, 10, 0);
        List<RecipeEntry> result = await service.GetRecipeList(filter);
        #endregion

        #region Assert that the image is downloaded
        Assert.Multiple(() =>
        {
            foreach (string path in expectedFilePaths)
            {
                Assert.That(File.Exists(path), Is.True);
            }
        });
        #endregion
    }

    [Test]
    public async Task WillCorrectlyChangePathOfImages()
    {
        #region Arrange
        #region create image paths
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        string[] expectedFilePaths = [
            Path.Combine(appDataPath, "asdafc.png"),
            Path.Combine(appDataPath, "agdgd.png")
        ];
        #endregion

        #region create a mock that also contains a route where an image is returned
        Mock<HttpMessageHandler> mockHttpMessageHandler = new();
        mockHttpMessageHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
        {
            #region image response
            string path = request.RequestUri?.AbsolutePath ?? "";
            if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                ByteArrayContent imageContent = new(pngBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = imageContent
                };
            }
            #endregion
            #region default data response
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(exampleJson)
            };
            #endregion
        });
        #endregion

        #region initialize the service
        HttpClient mockHttpClient = new(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };
        OnlineRecipeListService service = new(mockHttpClient);
        #endregion
        #endregion

        #region Act
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], null, 10, 0);
        List<RecipeEntry> result = await service.GetRecipeList(filter);
        #endregion

        #region Assert that the image is downloaded
        Assert.Multiple(() =>
        {
            Assert.That(result[0].ImagePath, Is.EqualTo(expectedFilePaths[0]));
            Assert.That(result[1].ImagePath, Is.EqualTo(expectedFilePaths[1]));
        });
        #endregion
    }

    [Test]
    public async Task GetCategories_ShouldBuildRightUrlAndReturnCorrectResults()
    {
        #region Arrange
        List<string> expectedCategories = ["category1", "category2"];
        string categoriesJson = "{ \"categories\": [\"category1\", \"category2\"] }";

        int limit = 10;
        int offset = 0;

        Mock<HttpMessageHandler> mockHttpMessageHandler = new();
        mockHttpMessageHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(categoriesJson)
            };
        });

        HttpClient mockHttpClient = new(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };
        OnlineRecipeListService service = new(mockHttpClient);
        #endregion

        #region Act
        List<string> returnedCategories = await service.GetCategories(limit, offset);
        #endregion

        #region Assert
        // Assert that the result is as expected
        Assert.That(expectedCategories, Is.EqualTo(returnedCategories));

        // Assert that the correct URL was called
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.AbsolutePath == "/categories" &&
                req.RequestUri.Query.Contains($"count={limit}") &&
                req.RequestUri.Query.Contains($"offset={offset}")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
        #endregion
    }
}