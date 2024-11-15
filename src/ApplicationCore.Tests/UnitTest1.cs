using ApplicationCore.Common.Types;
using ApplicationCore.Model;

namespace ApplicationCore.Tests;

public class OnlineAPIHandlerTests
{
    [SetUp]
    public void Setup()
    {
        OnlineAPIHandler onlineAPIHandler= new();
    }

    // Mock http client
    // test urls
    // test if recipe entries are extracted correctly

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}