using ApplicationCore.Common.Types;
using ApplicationCore.Model;
using Moq;
using Moq.Protected;
using System.Net;

namespace ApplicationCore.Tests;

public class OnlineRecipeListServiceTests
{
#pragma warning disable CS8618
    OnlineRecipeListService onlineRecipeListService;
#pragma warning restore CS8618

    private string exampleJson = @"{
    ""recipes"": [{
        ""hash"": ""asdafc"",
        ""title"": ""title1"",
        ""description"": ""description1"",
        ""image_path"": ""imagePath1"",
        ""categories"": [""category1"", ""category2""],
        ""cooking_time"": 15
    },
    {
        ""hash"": ""agdgd"",
        ""title"": ""title2"",
        ""description"": ""description2"",
        ""image_path"": ""imagePath2"",
        ""categories"": [""category1"", ""category2""],
        ""cooking_time"": 30
    }]
    }";


    [SetUp]
    public void Setup()
    {
        HttpClient httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:2222/list/")
        };
        onlineRecipeListService = new(httpClient);
    }

    [Test]
    public void WillLeaveOutDefaults_WhenBuildingUrl()
    {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], 10, 0);
        string url = onlineRecipeListService.BuildUrl(filter);
        Assert.Multiple(() => {
            Assert.That(url, Does.Not.Contain("order_by=title"));
            Assert.That(url, Does.Not.Contain("order_by=cooking_time"));
            Assert.That(url, Does.Not.Contain("order=asc"));
            Assert.That(url, Does.Not.Contain("order=desc"));
        });
    }

    [Test]
    public void WillShowOrderAndOrderBy_WhenNotDefault() {
        Filter filter = new(OrderBy.COOKINGTIME, Order.DESCENDING, ["category1", "category2"], 10, 0);
        string url = onlineRecipeListService.BuildUrl(filter);
        Assert.Multiple(() => {
            Assert.That(url, Does.Not.Contain("order_by=title"));
            Assert.That(url, Does.Contain("order_by=cooking_time"));
            Assert.That(url, Does.Not.Contain("order=asc"));
            Assert.That(url, Does.Contain("order=desc"));
        });
    }

    [Test]
    public void WillListCategories_WhenBuildingUrl() {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], 10, 0);
        string url = onlineRecipeListService.BuildUrl(filter);
        Assert.That(url, Does.Contain("categories=category1,category2"));
    }

    [Test]
    public void WillStartWithQuestionMark_WhenBuildingUrl() {
        Filter filter = new(OrderBy.TITLE, Order.ASCENDING, ["category1", "category2"], 10, 0);
        string url = onlineRecipeListService.BuildUrl(filter);
        Assert.That(url, Does.StartWith("?"));
    }

    [Test]
    public async Task WillCorrectlyExtractRecipeEntriesFromJson() {
        // Setup
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
            BaseAddress = new Uri("http://localhost:2222/list/")
        };
        OnlineRecipeListService service = new(mockHttpClient);

        // Execute
        Filter filter = new(OrderBy.COOKINGTIME, Order.DESCENDING, ["category1", "category2"], 10, 0);
        List<RecipeEntry> result = await service.GetOnlineRecipeList(filter);

        // Assert
        Assert.Multiple(() => {
            Assert.That(result, Has.Count.EqualTo(2));
            // first recipe entry
            Assert.Multiple(() => {
                Assert.That(result[0].hash, Is.EqualTo("asdafc"));
                Assert.That(result[0].title, Is.EqualTo("title1"));
                Assert.That(result[0].description, Is.EqualTo("description1"));
                Assert.That(result[0].imagePath, Is.EqualTo("imagePath1"));
                Assert.That(result[0].cookingTime, Is.EqualTo(15));
                Assert.That(result[0].categories, Does.Contain("category1"));
                Assert.That(result[0].categories, Does.Contain("category2"));
            });
            // second recipe entry
            Assert.Multiple(() => {
                Assert.That(result[1].hash, Is.EqualTo("agdgd"));
                Assert.That(result[1].title, Is.EqualTo("title2"));
                Assert.That(result[1].description, Is.EqualTo("description2"));
                Assert.That(result[1].imagePath, Is.EqualTo("imagePath2"));
                Assert.That(result[1].cookingTime, Is.EqualTo(30));
                Assert.That(result[1].categories, Does.Contain("category1"));
                Assert.That(result[1].categories, Does.Contain("category2"));
            });
        });
    }

    // will correctly responsd when encountering erros during HttpRequests
}