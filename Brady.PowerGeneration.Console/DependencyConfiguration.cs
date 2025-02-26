﻿using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Core.Models;
using Brady.PowerGeneration.Core.Services.HelperServices;
using Brady.PowerGeneration.Core.Services.ProcessingServices;
using Brady.PowerGeneration.Core.Validators;
using Brady.PowerGeneration.Infrastructure.Configuration;
using Brady.PowerGeneration.Infrastructure.FileProcessing;
using Brady.PowerGeneration.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brady.PowerGeneration.Console
{
    public static class DependencyConfiguration
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services,IConfiguration configuration)
        {
            // Bind configuration settings
            services.Configure<PowerGenerationSettings>(
                configuration.GetSection("PowerGeneration"));

            // Register Core services
            services.AddScoped<IGenerationReportProcessor, GenerationReportProcessor>();
            services.AddScoped<IValueFactorProvider, GenerationValueCalculator>();
            services.AddScoped<IEmissionFactorProvider, EmissionsCalculator>();

            services.AddScoped<IXmlDataValidator<GenerationReportDtoIn>, GenerationReportValidator>();
            services.AddScoped<IXmlDataValidator<ReferenceData>, ReferenceDataValidator>();

            // Register Infrastructure services
            services.AddScoped<IXmlRepository<GenerationReportDtoIn>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<XmlRepository<GenerationReportDtoIn>>>();
                var validator = sp.GetRequiredService<IXmlDataValidator<GenerationReportDtoIn>>();
                return new XmlRepository<GenerationReportDtoIn>(logger, validator);
            });

            services.AddScoped<IXmlRepository<GenerationReportDtoOut>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<XmlRepository<GenerationReportDtoOut>>>();
                
                return new XmlRepository<GenerationReportDtoOut>(logger, null!);
            });

            services.AddScoped<IXmlRepository<ReferenceData>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<XmlRepository<ReferenceData>>>();
                var validator = sp.GetRequiredService<IXmlDataValidator<ReferenceData>>();
                return new XmlRepository<ReferenceData>(logger, validator);
            });
            services.AddScoped<IFileMonitor, XmlFileMonitor>();

            return services;
        }
    }
}
