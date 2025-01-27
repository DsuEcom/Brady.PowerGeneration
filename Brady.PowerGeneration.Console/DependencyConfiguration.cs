using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Core.Models;
using Brady.PowerGeneration.Infrastructure.Configuration;
using Brady.PowerGeneration.Infrastructure.FileProcessing;
using Brady.PowerGeneration.Infrastructure.Interfaces;
using Brady.PowerGeneration.Infrastructure.Repositories;
using Brady.PowerGeneration.Services.Calculators;
using Brady.PowerGeneration.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Console
{
    public static class DependencyConfiguration
    {
        public static IServiceCollection AddDependencies(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuration
            services.Configure<PowerGenerationSettings>(
                configuration.GetSection("PowerGeneration"));

            // XML Processing
            services.AddScoped<IXmlProcessingStrategy<GenerationReport>, GenerationReportStrategy>();
            services.AddScoped<IXmlProcessingStrategy<GenerationOutput>, GenerationOutputStrategy>();

            // Calculators
            services.AddScoped<IValueFactorProvider, GenerationValueCalculator>();
            services.AddScoped<IEmissionFactorProvider, EmissionsCalculator>();

            // Repositories
            services.AddScoped<XmlRepository>();

            // Services
            services.AddScoped<IGenerationReportProcessor, GenerationReportService>();

            // File Monitoring
            services.AddScoped(typeof(XmlFileMonitor<>));
            services.AddScoped<IFileMonitor>(sp =>
            {
                var strategy = sp.GetRequiredService<IXmlProcessingStrategy<GenerationReport>>();
                var processor = sp.GetRequiredService<IGenerationReportProcessor>();

                return new XmlFileMonitor<GenerationReport>(strategy,
                    async report => await processor.ProcessReportAsync(report));
            });

            return services;
        }
    }
}
