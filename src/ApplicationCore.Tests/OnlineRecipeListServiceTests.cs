using ApplicationCore.Common.Types;
using ApplicationCore.Model;

namespace ApplicationCore.Tests;

public class OnlineRecipeListServiceTests
{
    OnlineRecipeListService onlineRecipeListService;

    [SetUp]
    public void Setup()
    {
        HttpClient httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:2222/list/")
        };
        onlineRecipeListService = new(httpClient);
    }

    // Mock http client
    // test urls
    // test if recipe entries are extracted correctly

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
}