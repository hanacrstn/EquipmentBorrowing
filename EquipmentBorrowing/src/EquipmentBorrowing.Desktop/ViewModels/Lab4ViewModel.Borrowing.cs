using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class Lab4ViewModel
{
    private LoanCriteria currentLoans = new();
    private string loanTitle = "All borrowings";

    public LoanStatusFilter[] StatusOptions { get; } = Enum.GetValues<LoanStatusFilter>();
    public LoanSort[] SortOptions { get; } = Enum.GetValues<LoanSort>();

    [ObservableProperty] private StudentRow? borrower;
    [ObservableProperty] private ItemRow? firstItem;
    [ObservableProperty] private ItemRow? secondItem;
    [ObservableProperty] private ItemRow? thirdItem;
    [ObservableProperty] private DateTimeOffset? dueDate;
    [ObservableProperty] private IReadOnlyList<LoanRow> loans = Array.Empty<LoanRow>();
    [ObservableProperty] private LoanRow? selectedLoan;
    [ObservableProperty] private bool confirmReturn;
    [ObservableProperty] private string? searchText;
    [ObservableProperty] private LoanStatusFilter statusFilter;
    [ObservableProperty] private LoanSort sortField;
    [ObservableProperty] private bool descending = true;
    [ObservableProperty] private string loanState = "";
    [ObservableProperty] private StudentRow? historyStudent;
    [ObservableProperty] private ItemRow? historyItem;

    partial void OnSelectedLoanChanged(LoanRow? value) => ConfirmReturn = false;

    [RelayCommand]
    private void ClearCheckout()
    {
        Borrower = null;
        FirstItem = null;
        SecondItem = null;
        ThirdItem = null;
        DueDate = null;
    }

    [RelayCommand] private Task IssueOneAsync() => IssueAsync(false);
    [RelayCommand] private Task IssueBatchAsync() => IssueAsync(true);

    private Task IssueAsync(bool multiple) => SaveAsync(() =>
    {
        ItemRow?[] selected = multiple
            ? new[] { FirstItem, SecondItem, ThirdItem }
            : new[] { FirstItem };
        var ids = selected.OfType<ItemRow>().Select(item => item.Id).ToArray();
        DateOnly? due = DueDate is { } date ? DateOnly.FromDateTime(date.Date) : null;
        return service.IssueAsync(Borrower?.Id, ids, due, multiple);
    });

    [RelayCommand]
    private Task ReturnSelectedAsync() => SaveAsync(() =>
    {
        if (!ConfirmReturn)
            throw new Lab4Problem("Confirm the selected return before submitting.");
        return service.ReturnAsync(SelectedLoan?.Id);
    });

    [RelayCommand]
    private Task ActiveLoansAsync() =>
        ShowLoansAsync(new LoanCriteria(Status: LoanStatusFilter.Active), "Active borrowings");

    [RelayCommand]
    private Task SearchLoansAsync() =>
        ShowLoansAsync(new LoanCriteria(SearchText, StatusFilter, SortField, Descending), "Search results");

    [RelayCommand]
    private Task ClearSearchAsync()
    {
        SearchText = null;
        StatusFilter = LoanStatusFilter.All;
        SortField = LoanSort.Checkout;
        Descending = true;
        return ShowLoansAsync(new LoanCriteria(), "All borrowings");
    }

    [RelayCommand]
    private Task StudentHistoryAsync()
    {
        var subject = HistoryStudent;
        return subject is null
            ? RunAsync(() => throw new Lab4Problem("Select a student to view history."))
            : ShowLoansAsync(new LoanCriteria(StudentId: subject.Id),
                $"Student history · {subject.Number} {subject.Name}");
    }

    [RelayCommand]
    private Task EquipmentHistoryAsync()
    {
        var subject = HistoryItem;
        return subject is null
            ? RunAsync(() => throw new Lab4Problem("Select equipment to view history."))
            : ShowLoansAsync(new LoanCriteria(EquipmentId: subject.Id),
                $"Equipment history · {subject.Code} {subject.Name}");
    }

    [RelayCommand] private Task RetryLoansAsync() => ShowLoansAsync(currentLoans, loanTitle);

    private Task ShowLoansAsync(LoanCriteria criteria, string title) => RunAsync(async () =>
    {
        loanTitle = title;
        await ReloadLoansAsync(criteria);
        return "Borrowing records loaded.";
    });

    private async Task ReloadLoansAsync(LoanCriteria criteria)
    {
        currentLoans = criteria;
        SelectedLoan = null;
        Loans = Array.Empty<LoanRow>();
        LoanState = $"{loanTitle} · Loading…";
        try
        {
            Loans = await service.LoansAsync(criteria);
            var empty = criteria.StudentId is not null || criteria.EquipmentId is not null
                ? "No borrowing history." : "No borrowings match.";
            LoanState = $"{loanTitle} · {(Loans.Count == 0 ? empty : $"{Loans.Count} result(s)")}";
        }
        catch (Lab4Problem problem)
        {
            LoanState = $"{loanTitle} · {problem.Message}";
            throw;
        }
    }
}