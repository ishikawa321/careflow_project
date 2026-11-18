using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CareFlow.Data;
using CareFlow.Services;
using CareFlow.ViewModels;
using CareFlow.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlow;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            services.AddDbContextFactory<CareFlowDbContext>(options =>
                options.UseSqlite(DatabaseSettings.ConnectionString));

            services.AddSingleton<AvailabilityService>();
            services.AddSingleton<BookingManager>();
            services.AddSingleton<BookingQueryService>();
            services.AddSingleton<CareFlowViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            desktop.MainWindow = new MainWindow
            {
                DataContext =
                    _serviceProvider.GetRequiredService<CareFlowViewModel>()
            };

            desktop.Exit += (_, _) =>
            {
                _serviceProvider?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}