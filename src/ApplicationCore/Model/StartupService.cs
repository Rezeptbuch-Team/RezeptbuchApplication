namespace ApplicationCore.Model;

public class StartupService
{
    public static void CreateAppDataFolder()
    {
        string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
    }

    public static void CheckForConflicts()
    {
        throw new NotImplementedException();
    }

    public static void CheckForOrphanedFiles()
    {
        throw new NotImplementedException();
    }

    public static void CheckForPublishableUpdates()
    {
        throw new NotImplementedException();
    }
}