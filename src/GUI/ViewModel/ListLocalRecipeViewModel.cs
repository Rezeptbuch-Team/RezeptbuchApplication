using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListLocalRecipeViewModel : ListRecipeViewModel
{
    private readonly ILocalRecipeListService _localRecipeListService;
    
    [ObservableProperty] 
    private string _text;
    
    public ListLocalRecipeViewModel(ILocalRecipeListService recipeListService) : base(recipeListService)
    {
        _localRecipeListService = recipeListService;
        Text = "Test DataBinding";
    }
}