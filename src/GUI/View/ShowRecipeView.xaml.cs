using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUI.ViewModel;

namespace GUI.View;

public partial class ShowRecipeView : ContentPage
{
    public ShowRecipeView(ShowRecipeViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}