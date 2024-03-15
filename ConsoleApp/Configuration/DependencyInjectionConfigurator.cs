using Library.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp.Configuration;

internal static class DependencyInjectionConfigurator
{
    public static IServiceProvider Setup()
    {
        var serviceProvider = new ServiceCollection()
            .SetupLibraryClasses()
            .SetupNLog()
            .BuildServiceProvider();

        return serviceProvider;
    }
}
