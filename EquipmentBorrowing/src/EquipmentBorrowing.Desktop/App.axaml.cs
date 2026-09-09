using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            equipmentRepository.Seed(new Equipment(100, "Digital Multimeter"));
            equipmentRepository.Seed(new Equipment(101, "Oscilloscope", isAvailable: false));

            var equipmentViewModel = new EquipmentViewModel(equipmentRepository);
            await equipmentViewModel.LoadCommand.ExecuteAsync(null);

            desktop.MainWindow = new MainWindow
            {
                Content = new EquipmentView { DataContext = equipmentViewModel }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}