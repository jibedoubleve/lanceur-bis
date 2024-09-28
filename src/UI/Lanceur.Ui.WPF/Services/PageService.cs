using System.Windows;
using Microsoft.Extensions.Logging;
using Wpf.Ui;

namespace Lanceur.Ui.WPF.Services;

// from src/Wpf.Ui.Demo.Mvvm/Services/PageService.cs
public class PageService : IPageService
{
    /// <summary>
    /// Service which provides the instances of pages.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<PageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageService"/> class and attaches the <see cref="IServiceProvider"/>.
    /// </summary>
    public PageService(IServiceProvider serviceProvider, ILogger<PageService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public T? GetPage<T>()
        where T : class
    {
        if (!typeof(FrameworkElement).IsAssignableFrom(typeof(T)))
        {
            throw new InvalidOperationException("The page should be a WPF control.");
        }

        return (T?)_serviceProvider.GetService(typeof(T));
    }

    /// <inheritdoc />
    public FrameworkElement? GetPage(Type pageType)
    {
        if (!typeof(FrameworkElement).IsAssignableFrom(pageType))
        {
            throw new InvalidOperationException("The page should be a WPF control.");
        }

        _logger.LogTrace("Get page {Page}", pageType.Name);
        return _serviceProvider.GetService(pageType) as FrameworkElement;
    }
}
