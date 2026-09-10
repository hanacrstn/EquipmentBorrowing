using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly BorrowEquipmentService _borrowEquipmentService;

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        IStudentRepository studentRepository,
        BorrowEquipmentService borrowEquipmentService)
    {
        _equipmentRepository = equipmentRepository;
        _studentRepository = studentRepository;
        _borrowEquipmentService = borrowEquipmentService;
    }

    public ObservableCollection<Equipment> Equipment { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private DateTimeOffset expectedReturnDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private string? statusMessage;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var equipmentItems = await _equipmentRepository.GetAllAsync();
        Equipment.Clear();
        foreach (var item in equipmentItems)
            Equipment.Add(item);

        var studentItems = await _studentRepository.GetAllAsync();
        Students.Clear();
        foreach (var student in studentItems)
            Students.Add(student);
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            return;
        }

        if (SelectedEquipment is null)
        {
            StatusMessage = "Please select equipment.";
            return;
        }

        if (ExpectedReturnDate.Date < DateTime.Today)
        {
            StatusMessage = "Expected return date cannot be in the past.";
            return;
        }

        var dateBorrowed = DateOnly.FromDateTime(DateTime.Today);
        var dueDate = DateOnly.FromDateTime(ExpectedReturnDate.Date);

        var result = await _borrowEquipmentService.ExecuteAsync(
            SelectedStudent.Id, SelectedEquipment.Id, dateBorrowed, dueDate);

        if (result.Success)
        {
            StatusMessage = $"Borrowing #{result.BorrowingId} approved.";
            await LoadAsync();
        }
        else
        {
            StatusMessage = $"Could not borrow: {result.Error}";
        }
    }
}