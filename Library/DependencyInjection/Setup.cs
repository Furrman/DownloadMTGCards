using Library.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Library.DependencyInjection;

public static class Setup
{
    public static IServiceCollection SetupLibraryClasses(this IServiceCollection services)
    {
        return services
            .AddScoped<ArchidektPrinter>()
            .AddScoped<ArchidektApiClient>()
            .AddScoped<ScryfallApiClient>()
            .AddScoped<CardListFileParser>()
            .AddScoped<WordGenerator>()
            .AddScoped<FileManager>()
        ;
    }
}