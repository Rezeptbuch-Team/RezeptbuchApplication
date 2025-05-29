using ApplicationCore.Model;
using ApplicationCore.Tests.Helpers;

namespace ApplicationCore.Tests.Tests;

[TestFixture]
public class StartupServiceTests
{
    [Test]
    public void ShouldCreateDataFolder_IfNotExists()
    {
        string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        if (Directory.Exists(dirPath)) Directory.Delete(dirPath, recursive: true);

        StartupService.CreateAppDataFolder(FileHelper.GetAppDataPath());

        Assert.That(Directory.Exists(dirPath), Is.True);
    }
}