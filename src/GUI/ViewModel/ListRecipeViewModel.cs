using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListRecipeViewModel : ObservableObject
{
    private readonly IRecipeListService _recipeListService;
    
    [ObservableProperty] 
    private List<RecipeEntry> _recipeEntries;

    [ObservableProperty] 
    private List<string> _orderOptions;
    
    [ObservableProperty]
    private string _selectedOrderOption;

    [ObservableProperty] 
    private List<string>? _availableFilterOptions;
    
    [ObservableProperty] 
    private List<string>? _selectedFilters;
    
    private Filter CurrentFilter => new Filter((OrderBy)Enum.Parse(typeof(OrderBy), SelectedOrderOption), Order.ASCENDING, SelectedFilters, null, 10, 0);
    
    protected ListRecipeViewModel(IRecipeListService recipeListService)
    {
        _recipeListService = recipeListService;
        RecipeEntries = [];
        OrderOptions= Enum.GetNames(typeof(OrderBy)).ToList();
        SelectedOrderOption = OrderOptions.First();
        //AvailableFilterOptions get from _recipeListService
        _ = RefreshRecipes();
    }

    
    private async Task RefreshRecipes()
    {
        try
        {
            RecipeEntries = await _recipeListService.GetRecipeList(CurrentFilter);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}