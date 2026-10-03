using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            var services = new ServiceCollection();

            services.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
                options.UseSqlite("Data Source=equipmentborrowing.db"));

            services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
            services.AddSingleton<IStudentRepository, EfStudentRepository>();
            services.AddSingleton<IBorrowingRepository, EfBorrowingRepository>();

            services.AddTransient<BorrowEquipmentService>();
            services.AddTransient<ReturnEquipmentService>();

            services.AddTransient<EquipmentViewModel>();
            services.AddTransient<BorrowingsViewModel>();
            services.AddTransient<MainWindowViewModel>();

            var provider = services.BuildServiceProvider();


            var contextFactory = provider.GetRequiredService<IDbContextFactory<EquipmentBorrowingDbContext>>();
            await using (var context = await contextFactory.CreateDbContextAsync())
            {
                await context.Database.MigrateAsync();
                await DatabaseSeeder.SeedAsync(context);
            }

            var mainWindowViewModel = provider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}