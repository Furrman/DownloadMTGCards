﻿using Library.Clients;
using Library.IO;
using Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Library.DependencyInjection;

public static class Setup
{
    public static IServiceCollection SetupLibraryClasses(this IServiceCollection services)
    {
        return services
            .AddScoped<IArchidektPrinter, ArchidektPrinter>()
            .AddScoped<ArchidektApiClient>()
            .AddScoped<ScryfallApiClient>()
            .AddScoped<CardListFileParser>()
            .AddScoped<IMagicCardService, MagicCardService>()
            .AddScoped<WordGeneratorService>()
            .AddScoped<ILanguageService, LanguageService>()
            .AddScoped<IFileManager, FileManager>()
        ;
    }
}