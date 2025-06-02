using GUI.View;
using Microsoft.Maui.Controls;

namespace GUI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		Routing.RegisterRoute(nameof(ShowRecipeView), typeof(ShowRecipeView));
		Routing.RegisterRoute(nameof(NavigateThroughRecipeView), typeof(NavigateThroughRecipeView));
	}
}
