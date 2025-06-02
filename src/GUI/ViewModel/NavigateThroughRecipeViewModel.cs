using ApplicationCore.Common.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GUI.ViewModel;

[QueryProperty(nameof(Recipe), "Recipe")]
[QueryProperty(nameof(Int32), "Servings")]
public partial class NavigateThroughRecipeViewModel : ObservableObject, IQueryAttributable
{
    public Recipe Recipe
    {
        set
        {
            Title = value.Title;
            _instructions = value.GetInstructions(_servings);
        }
    }
    
    private List<Instruction> _instructions = [];
    
    private int _servings;

    [ObservableProperty]
    private string _title = string.Empty;
    
    public int CurrentStepIndex
    {
        get => _currentStepIndex;
        set
        {
            _currentStepIndex = value;
            StepDescription = _instructions[value].ToString();
            PreviousStepCommand.NotifyCanExecuteChanged();
            NextStepCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CurrentStepIndex));
        }
    }
    private int _currentStepIndex;
    
    private bool HasPreviousStep => CurrentStepIndex > 0;
    private bool HasNextStep => CurrentStepIndex < (_instructions.Count) - 1;
    
    [ObservableProperty] 
    private string _stepDescription = string.Empty;
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _servings = (int)(query["Servings"] as int?)!;
        Recipe = (query["Recipe"] as Recipe)!;
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