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

            services.AddDbContext<EquipmentBorrowingDbContext>(options =>
                options.UseSqlite("Data Source=equipmentborrowing.db"));

            services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
            services.AddScoped<IStudentRepository, EfStudentRepository>();
            services.AddScoped<IBorrowingRepository, EfBorrowingRepository>();

            services.AddTransient<BorrowEquipmentService>();
            services.AddTransient<ReturnEquipmentService>();

            services.AddTransient<EquipmentViewModel>();
            services.AddTransient<BorrowingsViewModel>();
            services.AddTransient<MainWindowViewModel>();

            var provider = services.BuildServiceProvider();

            // Apply any pending migrations, then seed — both are idempotent,
            // safe to run on every launch.
            using (var scope = provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EquipmentBorrowingDbContext>();
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