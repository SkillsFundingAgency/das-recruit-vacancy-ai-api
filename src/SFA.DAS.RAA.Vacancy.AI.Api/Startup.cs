using Asp.Versioning;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.RAA.Vacancy.AI.Api.AppStart;
using SFA.DAS.RAA.Vacancy.AI.Api.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.RAA.Vacancy.AI.Api;

[ExcludeFromCodeCoverage]
internal class Startup
{
    private readonly string _environmentName;
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        _environmentName = configuration["EnvironmentName"]!;

        if (_environmentName == "INTEGRATION")
        {
            Configuration = configuration;
            return;
        }

        var config = new ConfigurationBuilder()
            .AddConfiguration(configuration);
            /*.AddAzureTableStorage(options =>
            {
                options.ConfigurationNameIncludesVersionNumber = true;
                options.ConfigurationKeys = configuration["ConfigNames"]!.Split(",");
                options.EnvironmentName = _environmentName;
                options.PreFixConfigurationKeys = false;
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
            });*/
        Console.WriteLine("DENCH");
        Console.WriteLine(configuration["ConfigurationStorageConnectionString"]);
        //Console.WriteLine(OptionsBuilderConfigurationExtensions.ConfigurationKeys)

    #if DEBUG
        config.AddJsonFile("appsettings.Development.json", true);
    #endif
        Configuration = config.Build();
    }

    private bool IsEnvironmentLocalOrDev =>
        _environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase)
        || _environmentName.Equals("DEV", StringComparison.CurrentCultureIgnoreCase)
        || _environmentName.Equals("TEST", StringComparison.CurrentCultureIgnoreCase);

    public void ConfigureServices(IServiceCollection services)
    {
        if (!IsEnvironmentLocalOrDev)
        {
            var azureAdConfiguration = Configuration
                .GetSection("AzureAd")
                .Get<AzureActiveDirectoryConfiguration>();

            var policies = new Dictionary<string, string>
            {
                { PolicyNames.Default, "Default" },
            };
            services.AddAuthentication(azureAdConfiguration, policies);
            services
                .AddHealthChecks();
        }

        services
            .AddMvc(o =>
            {
                if (!IsEnvironmentLocalOrDev)
                {
                    o.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>
                    {
                        Capacity = 0
                    }));
                }
                o.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

        services.AddConfigurationOptions(Configuration);
        services.AddApplicationDependencies();
        services.AddOpenTelemetryRegistration(Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!);
        services.ConfigureHealthChecks();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SFA.DAS.RAA.Vacancy.AI.Api", Version = "v1" });
            c.OperationFilter<SwaggerVersionHeaderFilter>();
            c.DocumentFilter<HealthChecksFilter>();
        });
        services.AddApiVersioning(opt =>
        {
            opt.ApiVersionReader = new HeaderApiVersionReader("X-Version");
            opt.DefaultApiVersion = new ApiVersion(1, 0);
        });
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
        app.UseRouting();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SFA.DAS.RAA.Vacancy.AI.Api v1");
            options.RoutePrefix = string.Empty;
        });
        app.UseAuthentication();
        app.UseHealthChecks();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
