﻿using Microsoft.Extensions.Logging;
using ApplicationCore.Model;
using ApplicationCore.Interfaces;
using GUI.View;
using GUI.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace GUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// needed for application core http clients
		builder.Services.AddHttpClient<IOnlineRecipeListService, OnlineRecipeListService>(client => {
			client.BaseAddress = new Uri("http://localhost:2222/list/"); // replace with url from configuration. for example: builder.Configuration["base_url"]
		});

		builder.Services.AddSingleton<IDatabaseService, SqliteService>();
		builder.Services.AddSingleton<ILocalRecipeListService, LocalRecipeListService>();

		// add Views and ViewModels
		builder.Services.AddTransient<ListLocalRecipeView>();
		builder.Services.AddTransient<ListLocalRecipeViewModel>();

		builder.Services.AddTransient<ListRemoteRecipeView>();
		builder.Services.AddTransient<ListRemoteRecipeViewModel>();

		builder.Services.AddTransient<ShowRecipeView>();
		builder.Services.AddTransient<ShowRecipeViewModel>();
		
		builder.Services.AddTransient<NavigateThroughRecipeView>();
		builder.Services.AddTransient<NavigateThroughRecipeViewModel>();

		builder.Services.AddTransient<CreateRecipeView>();
		
		#if DEBUG
				builder.Logging.AddDebug();
		#endif

		return builder.Build();
	}
}
