using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class Lab4ViewModel : ObservableObject
{
    private readonly Lab4Service service;

    public Lab4ViewModel(Lab4Service service) => this.service = service;

    [ObservableProperty] private IReadOnlyList<StudentRow> students = Array.Empty<StudentRow>();
    [ObservableProperty] private IReadOnlyList<ItemRow> items = Array.Empty<ItemRow>();
    [ObservableProperty] private IReadOnlyList<ItemRow> catalog = Array.Empty<ItemRow>();
    [ObservableProperty] private StudentRow? selectedStudent;
    [ObservableProperty] private ItemRow? selectedItem;
    [ObservableProperty] private string? studentNumber;
    [ObservableProperty] private string? studentName;
    [ObservableProperty] private string? contact;
    [ObservableProperty] private string? itemCode;
    [ObservableProperty] private string? itemName;
    [ObservableProperty] private string? description;
    [ObservableProperty] private bool availableOnly;
    [ObservableProperty] private bool busy;
    [ObservableProperty] private string feedback = "Select Refresh to load records.";
    [ObservableProperty] private string catalogState = "";

    private bool needsRecovery;

    partial void OnSelectedStudentChanged(StudentRow? value)
    {
        StudentNumber = value?.Number;
        StudentName = value?.Name;
        Contact = value?.Contact;
    }

    partial void OnSelectedItemChanged(ItemRow? value)
    {
        ItemCode = value?.Code;
        ItemName = value?.Name;
        Description = value?.Description;
    }

    [RelayCommand]
    private void NewStudent()
    {
        SelectedStudent = null;
        OnSelectedStudentChanged(null);
    }

    [RelayCommand]
    private void NewItem()
    {
        SelectedItem = null;
        OnSelectedItemChanged(null);
    }

    [RelayCommand]
    private Task RegisterStudentAsync() =>
        SaveAsync(() => service.SaveStudentAsync(null, StudentNumber, StudentName, Contact));
    [RelayCommand]
    private Task UpdateStudentAsync() =>
        SaveAsync(() => service.SaveStudentAsync(
            SelectedStudent ?? throw new Lab4Problem("Select the student to update."),
            StudentNumber, StudentName, Contact));
    [RelayCommand]
    private Task PermitAsync() =>
        SaveAsync(() => service.SetEligibilityAsync(SelectedStudent, true));
    [RelayCommand]
    private Task RestrictAsync() =>
        SaveAsync(() => service.SetEligibilityAsync(SelectedStudent, false));
    [RelayCommand]
    private Task RegisterItemAsync() =>
        SaveAsync(() => service.SaveEquipmentAsync(null, ItemCode, ItemName, Description));
    [RelayCommand]
    private Task UpdateItemAsync() =>
        SaveAsync(() => service.SaveEquipmentAsync(
            SelectedItem ?? throw new Lab4Problem("Select the equipment to update."),
            ItemCode, ItemName, Description));

    [RelayCommand]
    private Task LoadCatalogAsync() => RunAsync(async () =>
    {
        CatalogState = "";
        Catalog = Array.Empty<ItemRow>();
        await ReloadCatalogAsync();
        return "Catalog loaded.";
    });

    private async Task ReloadCatalogAsync()
    {
        try
        {
            Catalog = await service.EquipmentAsync(AvailableOnly);
            CatalogState = Catalog.Count == 0 ? "No equipment matches." : $"{Catalog.Count} item(s).";
        }
        catch (Lab4Problem)
        {
            Catalog = Array.Empty<ItemRow>();
            CatalogState = "Catalog unavailable. Retry the load.";
            throw;
        }
    }

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(async () =>
    {
        if (needsRecovery)
        {
            loanTitle = "All borrowings";
            AvailableOnly = false;
        }
        await RefreshListsAsync();
        await ReloadLoansAsync(needsRecovery ? new LoanCriteria() : currentLoans);
        var recovered = needsRecovery;
        needsRecovery = false;
        return recovered
            ? "Connection restored. Review the refreshed catalog and borrowing records before resubmitting."
            : "Records refreshed.";
    });

    private async Task RefreshListsAsync()
    {
        var studentId = SelectedStudent?.Id;
        var itemId = SelectedItem?.Id;
        Students = await service.StudentsAsync();
        Items = await service.EquipmentAsync();
        SelectedStudent = Students.FirstOrDefault(s => s.Id == studentId);
        SelectedItem = Items.FirstOrDefault(e => e.Id == itemId);
        await ReloadCatalogAsync();
    }

    private Task SaveAsync(Func<Task<string>> write) => RunAsync(async () =>
    {
        if (needsRecovery)
            throw new Lab4Problem("Refresh after reconnecting and review the outcome before another write.");

        var message = await write();
        try
        {
            await RefreshListsAsync();
            await ReloadLoansAsync(currentLoans);
            return message;
        }
        catch (Lab4Problem ex)
        {
            return $"{message} Refresh failed: {ex.Message}";
        }
    });

    private async Task RunAsync(Func<Task<string>> action)
    {
        if (Busy)
            return;

        Busy = true;
        Feedback = "Loading or saving…";
        try
        {
            Feedback = await action();
        }
        catch (Lab4Problem ex)
        {
            needsRecovery |= ex.Unknown;
            Feedback = ex.Message;
            if (ex.Refresh && !needsRecovery)
            {
                try
                {
                    await RefreshListsAsync();
                    await ReloadLoansAsync(currentLoans);
                }
                catch (Lab4Problem)
                {
                    Feedback += " Refresh failed. Restore the connection and select Refresh.";
                }
            }
        }
        finally
        {
            Busy = false;
        }
    }
}