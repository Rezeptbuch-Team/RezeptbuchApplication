using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUI.ViewModel;

namespace GUI.View;

public partial class ListRemoteRecipeView : ContentPage
{
    public ListRemoteRecipeView(ListRemoteRecipeViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}