using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        CurrentView = _equipmentViewModel;
    }

    [ObservableProperty]
    private ObservableObject currentView;

    [RelayCommand]
    private async Task ShowEquipmentAsync()
    {
        CurrentView = _equipmentViewModel;
        await _equipmentViewModel.LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task ShowBorrowingsAsync()
    {
        CurrentView = _borrowingsViewModel;
        await _borrowingsViewModel.LoadCommand.ExecuteAsync(null);
    }
}