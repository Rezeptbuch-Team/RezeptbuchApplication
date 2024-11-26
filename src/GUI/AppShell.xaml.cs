using GUI.View;

namespace GUI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		Routing.RegisterRoute(nameof(LandingPageView), typeof(LandingPageView));
		Routing.RegisterRoute($"{nameof(LandingPageView)}/{nameof(ListLocalRecipeView)}", typeof(ListLocalRecipeView));
		
		InitializeComponent();
	}
}
