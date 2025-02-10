using Autofac;
using Autofac.Extensions.DependencyInjection;
using Aggregator.Api.Models;
using Aggregator.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Aggregator.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.Register(context =>
    {
        var config = context.Resolve<IConfiguration>();
        var baseUrl = config["ApiSettings:GeoapifyBaseUrl"];
        return new HttpClient { BaseAddress = new Uri(baseUrl!) };
    }).As<HttpClient>().Named<HttpClient>("GeoapifyClient").InstancePerLifetimeScope();

    containerBuilder.Register(context =>
    {
        var config = context.Resolve<IConfiguration>();
        var baseUrl = config["ApiSettings:WeatherBaseUrl"];
        return new HttpClient { BaseAddress = new Uri(baseUrl!) };
    }).As<HttpClient>().Named<HttpClient>("WeatherClient").InstancePerLifetimeScope();

    containerBuilder.Register(context =>
    {
        var config = context.Resolve<IConfiguration>();
        var baseUrl = config["ApiSettings:NewsBaseUrl"];
        return new HttpClient { BaseAddress = new Uri(baseUrl!) };
    }).As<HttpClient>().Named<HttpClient>("NewsClient").InstancePerLifetimeScope();

    containerBuilder.Register(context =>
    {
        var config = context.Resolve<IConfiguration>();
        var baseUrl = config["ApiSettings:PlacesApiBaseUrl"];
        return new HttpClient { BaseAddress = new Uri(baseUrl!) };
    }).As<HttpClient>().Named<HttpClient>("PlacesClient").InstancePerLifetimeScope();

    containerBuilder.RegisterType<GeoapifyService>()
        .As<IGeoapifyService>()
        .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient),
            (pi, ctx) => ctx.ResolveNamed<HttpClient>("GeoapifyClient"))
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<WeatherService>()
        .As<IWeatherService>()
        .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient),
            (pi, ctx) => ctx.ResolveNamed<HttpClient>("WeatherClient"))
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<NewsService>()
        .As<INewsService>()
        .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient),
            (pi, ctx) => ctx.ResolveNamed<HttpClient>("NewsClient"))
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<PlacesService>()
        .As<IPlacesService>()
        .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient),
            (pi, ctx) => ctx.ResolveNamed<HttpClient>("PlacesClient"))
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<AggregatorService>()
        .As<IAggregatorService>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<CacheService>()
        .As<ICacheService>()
        .SingleInstance();

    containerBuilder.RegisterAssemblyTypes(typeof(AggregatorController).Assembly)
        .Where(t => t.IsAssignableTo<ControllerBase>())
        .InstancePerLifetimeScope();
});

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();