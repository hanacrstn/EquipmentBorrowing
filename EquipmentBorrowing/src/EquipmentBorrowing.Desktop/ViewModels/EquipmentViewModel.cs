using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;

    public EquipmentViewModel(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public ObservableCollection<Equipment> Equipment { get; } = new();

    [ObservableProperty]
    private string? statusMessage;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var items = await _equipmentRepository.GetAllAsync();

        Equipment.Clear();
        foreach (var item in items)
            Equipment.Add(item);

        StatusMessage = Equipment.Count == 0 ? "No equipment records found." : null;
    }
}