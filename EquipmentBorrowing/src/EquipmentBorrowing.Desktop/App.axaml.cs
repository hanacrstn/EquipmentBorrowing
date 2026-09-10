using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            var equipmentRepository = new InMemoryEquipmentRepository();
            equipmentRepository.Seed(new Equipment(100, "Digital Multimeter"));
            equipmentRepository.Seed(new Equipment(101, "Oscilloscope", isAvailable: false));
            services.AddSingleton<IEquipmentRepository>(equipmentRepository);

            var studentRepository = new InMemoryStudentRepository();
            studentRepository.Seed(new Student(1, "Juan Dela Cruz"));
            studentRepository.Seed(new Student(2, "Maria Santos", isAllowedToBorrow: false));
            services.AddSingleton<IStudentRepository>(studentRepository);

            services.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

            services.AddTransient<BorrowEquipmentService>();
            services.AddTransient<ReturnEquipmentService>();

            services.AddTransient<EquipmentViewModel>();
            services.AddTransient<BorrowingsViewModel>();
            services.AddTransient<MainWindowViewModel>();

            var provider = services.BuildServiceProvider();

            var mainWindowViewModel = provider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}