using System.Collections.ObjectModel;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModel;

public partial class ListLocalRecipeViewModel : ListRecipeViewModel
{
    [ObservableProperty] private string _text;
    
    public ListLocalRecipeViewModel(IOnlineRecipeListService onlineRecipeListService) : base(onlineRecipeListService)
    {
        Text = "Test DataBinding";
    }
}