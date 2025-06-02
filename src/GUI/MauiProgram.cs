using Microsoft.Extensions.Logging;
using ApplicationCore.Model;
using ApplicationCore.Interfaces;
using GUI.View;
using GUI.ViewModel;
using UraniumUI;

namespace GUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseUraniumUI()
			.UseUraniumUIMaterial()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// needed for application core http clients
		builder.Services.AddHttpClient<IOnlineRecipeListService, OnlineRecipeListService>(client => {
			client.BaseAddress = new Uri("https://rezeptbuchapi.onrender.com/"); // replace with url from configuration. for example: builder.Configuration["base_url"]
		});
		builder.Services.AddHttpClient<IDownloadRecipeService, DownloadRecipeService>(client => {
			client.BaseAddress = new Uri("https://rezeptbuchapi.onrender.com/"); // replace with url from configuration. for example: builder.Configuration["base_url"]
		});

		string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
		StartupService.CreateAppDataFolder(appDataPath);

		builder.Services.AddSingleton<string>(appDataPath);
		builder.Services.AddSingleton<IDatabaseService>(sp =>
		{
			IDatabaseService databaseService = new SqliteService(appDataPath);
			databaseService.InitializeAsync().Wait();
			return databaseService;
		});

		builder.Services.AddSingleton<StartupService>();
		builder.Services.AddSingleton<IGetRecipeFromFileService, GetRecipeFromFileService>();
		builder.Services.AddSingleton<IGetLocalRecipeService, GetLocalRecipeService>();
		builder.Services.AddSingleton<ILocalRecipeListService, LocalRecipeListService>();
		builder.Services.AddSingleton<IOnlineIdentificationService, OnlineIdentificationService>();

		// add Views and ViewModels
		builder.Services.AddSingleton<ListLocalRecipeView>();
		builder.Services.AddSingleton<ListLocalRecipeViewModel>();

		builder.Services.AddSingleton<ListRemoteRecipeView>();
		builder.Services.AddSingleton<ListRemoteRecipeViewModel>();

		builder.Services.AddTransient<ShowRecipeView>();
		builder.Services.AddTransient<ShowRecipeViewModel>();
		
		builder.Services.AddTransient<NavigateThroughRecipeView>();
		builder.Services.AddTransient<NavigateThroughRecipeViewModel>();
		
		#if DEBUG
				builder.Logging.AddDebug();
		#endif

		MauiApp app = builder.Build();

		StartupService startupService = app.Services.GetRequiredService<StartupService>();
		startupService.CheckForOrphanedFiles().Wait();
		startupService.CheckForConflicts().Wait();

		return app;
	}
}
