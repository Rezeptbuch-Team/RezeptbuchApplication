using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListRemoteRecipeViewModel : ListRecipeViewModel
{
    public ListRemoteRecipeViewModel(IOnlineRecipeListService recipeListService) : base(recipeListService)
    {
    }
    
}