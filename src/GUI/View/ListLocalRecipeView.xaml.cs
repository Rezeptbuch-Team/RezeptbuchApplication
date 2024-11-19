using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUI.ViewModel;

namespace GUI.View;

public partial class ListLocalRecipeView : ContentPage
{
    public ListLocalRecipeView(ListLocalRecipeViewModel vm)
    {
        BindingContext = vm;
        
        InitializeComponent();
    }
}