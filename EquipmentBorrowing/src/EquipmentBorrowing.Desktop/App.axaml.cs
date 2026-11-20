using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
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
            services.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
                EquipmentBorrowingDbContextFactory.Configure(options));
            services.AddSingleton<ILab4Repository, Lab4Repository>();
            services.AddSingleton<Lab4Service>();
            services.AddSingleton<Lab4ViewModel>();
            var provider = services.BuildServiceProvider();
            desktop.Exit += (_, _) => provider.Dispose();

            var vm = provider.GetRequiredService<Lab4ViewModel>();
            var window = new MainWindow { DataContext = vm };
            window.Opened += async (_, _) => await vm.RefreshCommand.ExecuteAsync(null);
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}