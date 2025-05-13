using ApplicationCore.Common.Types;
using ApplicationCore.Model;
using Moq;
using Moq.Protected;
using System.Net;

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

        Assert.Multiple(() => {
            Assert.That(url, Does.Not.Contain("order_by=title"));
            Assert.That(url, Does.Not.Contain("order_by=cooking_time"));
            Assert.That(url, Does.Not.Contain("order=asc"));
            Assert.That(url, Does.Not.Contain("order=desc"));
        });
    }

    [Test]
    public void WillShowOrderAndOrderBy_WhenNotDefault() {
        Filter filter = new(OrderBy.COOKINGTIME, Order.DESCENDING, ["category1", "category2"], null, 10, 0);

        string url = onlineRecipeListService.BuildListUrl(filter);

        Assert.Multiple(() => {
            Assert.That(url, Does.Not.Contain("order_by=title"));
            Assert.That(url, Does.Contain("order_by=cooking_time"));
            Assert.That(url, Does.Not.Contain("order=asc"));
            Assert.That(url, Does.Contain("order=desc"));
        });
    }

    [Test]
    public void WillListCategories_WhenBuildingUrl() {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], null, 10, 0);
        
        string url = onlineRecipeListService.BuildListUrl(filter);
        
        Assert.That(url, Does.Contain("categories=category1,category2"));
    }

    [Test]
    public void WillStartCorrectly_WhenBuildingUrl() {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], null, 10, 0);

        string url = onlineRecipeListService.BuildListUrl(filter);
        
        Assert.That(url, Does.StartWith("/list?"));
    }

    [Test]
    public async Task WillCorrectlyExtractRecipeEntriesFromJson() {
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
        List<RecipeEntry> result = await service.GetOnlineRecipeList(filter);
        #endregion

        #region Assert
        Assert.Multiple(() => {
            Assert.That(result, Has.Count.EqualTo(2));
            #region first recipe entry
            Assert.Multiple(() => {
                Assert.That(result[0].hash, Is.EqualTo("asdafc"));
                Assert.That(result[0].title, Is.EqualTo("title1"));
                Assert.That(result[0].description, Is.EqualTo("description1"));
                Assert.That(result[0].cookingTime, Is.EqualTo(15));
                Assert.That(result[0].categories, Does.Contain("category1"));
                Assert.That(result[0].categories, Does.Contain("category2"));
            });
            #endregion
            #region second recipe entry
            Assert.Multiple(() => {
                Assert.That(result[1].hash, Is.EqualTo("agdgd"));
                Assert.That(result[1].title, Is.EqualTo("title2"));
                Assert.That(result[1].description, Is.EqualTo("description2"));
                Assert.That(result[1].cookingTime, Is.EqualTo(30));
                Assert.That(result[1].categories, Does.Contain("category1"));
                Assert.That(result[1].categories, Does.Contain("category2"));
            });
            #endregion
        });
        #endregion
    }

    [Test]
    public async Task WillTryToDownloadImages() {
        #region Arrange
        #region create a mock that also returns a url to an image
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
        #endregion

        #region initialize the service
        HttpClient mockHttpClient = new(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.server.com/")
        };
        OnlineRecipeListService service = new(mockHttpClient);
        #endregion
        #endregion

        // check that the imageurl is "accessed"

        #region Assert
        Assert.Fail("Not yet implemented");
        #endregion
    }

    [Test]
    public async Task WillCorrectlyDownloadImages() {
        // create a mock that returns a url to an image (like exampleJson but with a different image path)

        // check that the image is downloaded

        Assert.Fail("Not yet implemented");
    }

    [Test]
    public async Task WillCorrectlyChangePathOfImages() {
        // create a mock that returns a url to an image (like exampleJson but with a different image path)

        // check that the image is downloaded and the path is changed

        Assert.Fail("Not yet implemented");
    }

}