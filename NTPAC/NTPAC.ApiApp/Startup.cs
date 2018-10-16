using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NSwag.AspNetCore;
using NTPAC.Persistence.Cassandra.Facades;
using NTPAC.Persistence.Cassandra.Facades.Installers;
using NTPAC.Persistence.Cassandra.Migrations;
using NTPAC.Persistence.InMemory.Facades;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ApiApp
{
  public class Startup
  {
    public Startup(IConfiguration configuration) => this.Configuration = configuration;
    
    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseStaticFiles();

      if (env.IsDevelopment())
      {
        // Enable the Swagger UI middleware and the Swagger generator
        app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly, settings =>
        {
          settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
          settings.PostProcess = document =>
          {
            document.Info.Title   = "NTPAC API";
            document.Info.Version = "0.0.1";
            document.Host         = "localhost:5000";
            document.BasePath     = "/";
          };
        });
      }

      app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
      app.UseMvc();

      app.ApplicationServices.GetService<IDbSeed>()?.SeedDb();
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {     
      services.AddCors();
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      services.AddSingleton<ICaptureFacade, CaptureFacade>();
      services.AddSingleton<IL7ConversationFacade, L7ConversationFacade>();

      this.ConfigureRepositoryServices(services);
    }

    private void ConfigureRepositoryServices(IServiceCollection services)
    {
      var repositoryConfig   = this.Configuration.GetSection("Repository");
      var repositoryProvider = repositoryConfig["Provider"] ?? "inmemory";
      switch (repositoryProvider)
      {
          case "inmemory":
            InMemoryServiceInstaller.Install(services);
            services.AddSingleton<IDbSeed, DbSeed>();
            break;
          case "cassandra":
            var cassandraKeySpace = repositoryConfig["Keyspace"] ?? "ntpac";
            var cassandraContactPointsSection = repositoryConfig.GetSection("ContactPoints");
            var cassandraContactPoints = cassandraContactPointsSection.Get<string[]>() ?? new[] {cassandraContactPointsSection.Get<string>() ?? "localhost"};
            CassandraServiceInstaller.Install(services, cassandraKeySpace, cassandraContactPoints);
            break;
          default:
            throw new ArgumentException($"Unknown repository provider: {repositoryProvider}");
      }
    }
  }
}
