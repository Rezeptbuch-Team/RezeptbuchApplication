namespace ApplicationCore.Tests.Helpers;

public static class FileHelper
{
    public static string GetAppDataPath()
    {
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
    }
}