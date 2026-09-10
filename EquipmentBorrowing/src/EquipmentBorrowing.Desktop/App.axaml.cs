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
            studentRepository.Seed(new Student(1, "Keisha Montenegro"));
            studentRepository.Seed(new Student(2, "Hannah Montana"));

            var borrowingRepository = new InMemoryBorrowingRepository();

            var borrowEquipmentService = new BorrowEquipmentService(
                studentRepository, equipmentRepository, borrowingRepository);

            var returnEquipmentService = new ReturnEquipmentService(
                borrowingRepository, equipmentRepository);

            var equipmentViewModel = new EquipmentViewModel(
                equipmentRepository, studentRepository, borrowEquipmentService);

            var borrowingsViewModel = new BorrowingsViewModel(
                borrowingRepository, studentRepository, equipmentRepository, returnEquipmentService);

            await equipmentViewModel.LoadCommand.ExecuteAsync(null);

            var mainViewModel = new MainWindowViewModel(equipmentViewModel, borrowingsViewModel);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}