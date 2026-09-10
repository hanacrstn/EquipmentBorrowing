using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var equipmentRepository = new InMemoryEquipmentRepository();
            equipmentRepository.Seed(new Equipment(100, "Laptop"));
            equipmentRepository.Seed(new Equipment(101, "Keyboard", isAvailable: false));

            var studentRepository = new InMemoryStudentRepository();
            studentRepository.Seed(new Student(1, "Wendell Aha"));
            studentRepository.Seed(new Student(2, "Qin Qong", isAllowedToBorrow: false));

            var borrowingRepository = new InMemoryBorrowingRepository();

            var borrowEquipmentService = new BorrowEquipmentService(
                studentRepository, equipmentRepository, borrowingRepository);

            var equipmentViewModel = new EquipmentViewModel(
                equipmentRepository, studentRepository, borrowEquipmentService);
            await equipmentViewModel.LoadCommand.ExecuteAsync(null);

            desktop.MainWindow = new MainWindow
            {
                Content = new EquipmentView { DataContext = equipmentViewModel }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}