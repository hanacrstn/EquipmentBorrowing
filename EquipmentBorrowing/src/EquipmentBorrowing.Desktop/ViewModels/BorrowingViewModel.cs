using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ReturnEquipmentService _returnEquipmentService;

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        ReturnEquipmentService returnEquipmentService)
    {
        _borrowingRepository = borrowingRepository;
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _returnEquipmentService = returnEquipmentService;
    }

    public ObservableCollection<BorrowingDisplayItem> ActiveBorrowings { get; } = new();

    [ObservableProperty]
    private BorrowingDisplayItem? selectedBorrowing;

    [ObservableProperty]
    private string? statusMessage;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var active = await _borrowingRepository.GetAllActiveAsync();

        ActiveBorrowings.Clear();
        foreach (var borrowing in active)
        {
            var student = await _studentRepository.GetByIdAsync(borrowing.StudentId);
            var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId);

            ActiveBorrowings.Add(new BorrowingDisplayItem(
                borrowing.Id,
                borrowing.StudentId,
                student?.Name ?? "Unknown student",
                borrowing.EquipmentId,
                equipment?.Name ?? "Unknown equipment",
                borrowing.ExpectedReturnDate));
        }

        StatusMessage = ActiveBorrowings.Count == 0 ? "No active borrowings." : null;
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Please select a borrowing to return.";
            return;
        }

        var result = await _returnEquipmentService.ExecuteAsync(
            SelectedBorrowing.StudentId, SelectedBorrowing.EquipmentId);

        if (result.Success)
        {
            await LoadAsync();
            StatusMessage = "Equipment returned.";
        }
        else
        {
            StatusMessage = $"Could not return: {result.Error}";
        }
    }
}