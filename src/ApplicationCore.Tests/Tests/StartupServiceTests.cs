using ApplicationCore.Model;

namespace ApplicationCore.Tests;

[TestFixture]
public class StartupServiceTests
{
    [Test]
    public void ShouldCreateDataFolder_IfNotExists()
    {
        string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");

        if (Directory.Exists(dirPath)) Directory.Delete(dirPath, recursive: true);

        StartupService.CreateAppDataFolder();

        Assert.That(Directory.Exists(dirPath), Is.True);
    }
}