using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.View;

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
    private List<string> _availableFilterOptions = [];
    
    [ObservableProperty] 
    private ObservableCollection<string>? _selectedFilters = [];

    protected readonly string AppDataPath;
    
    private Filter CurrentFilter => new Filter((OrderBy)Enum.Parse(typeof(OrderBy), SelectedOrderOption), Order.ASCENDING, SelectedFilters?.ToList(), null, 10, 0);
    
    protected ListRecipeViewModel(IRecipeListService recipeListService, string appDataPath)
    {
        AppDataPath = appDataPath;
        _recipeListService = recipeListService;
        RecipeEntries = [];
        OrderOptions= Enum.GetNames(typeof(OrderBy)).ToList();
        SelectedOrderOption = OrderOptions.First();
        _ = RefreshRecipes();
    }

    [RelayCommand]
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