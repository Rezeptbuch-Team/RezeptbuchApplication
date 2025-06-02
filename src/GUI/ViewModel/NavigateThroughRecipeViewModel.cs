using ApplicationCore.Common.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GUI.ViewModel;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class NavigateThroughRecipeViewModel : ObservableObject, IQueryAttributable
{
    public Recipe Recipe
    {
        set => _recipe = value;
    }
    
    private Recipe? _recipe;

    public string? Title => _recipe?.Title;
    
    public int CurrentStepIndex
    {
        get => _currentStepIndex;
        set
        {
            _currentStepIndex = value;
            StepDescription = _recipe!.Instructions[value].ToString();
            PreviousStepCommand.NotifyCanExecuteChanged();
            NextStepCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CurrentStepIndex));
        }
    }
    private int _currentStepIndex;
    
    private bool HasPreviousStep => CurrentStepIndex > 0;
    private bool HasNextStep => CurrentStepIndex < (_recipe?.Instructions.Count ?? 0) - 1;
    
    [ObservableProperty] 
    private string _stepDescription = string.Empty;
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Recipe = (query[nameof(Recipe)] as Recipe)!;
        CurrentStepIndex = 0;
        OnPropertyChanged(nameof(Title));
    }
    
    [RelayCommand(CanExecute = nameof(HasPreviousStep))]
    private void PreviousStep()
    {
        --CurrentStepIndex;
    }
    
    [RelayCommand(CanExecute = nameof(HasNextStep))]
    private void NextStep()
    {
        ++CurrentStepIndex;
    }
}