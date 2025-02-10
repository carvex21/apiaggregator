using Aggregator.Api.Models;
using Aggregator.Api.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var containerBuilder = new ContainerBuilder();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

containerBuilder.Register(c =>
{
    var config = c.Resolve<IConfiguration>();
    var baseUrl = config["ApiSettings:GeoapifyBaseUrl"];
    var client = new HttpClient { BaseAddress = new Uri(baseUrl!) };
    return client;
}).As<HttpClient>().Named<HttpClient>("GeoapifyClient").InstancePerLifetimeScope();

containerBuilder.Register(c =>
{
    var config = c.Resolve<IConfiguration>();
    var baseUrl = config["ApiSettings:WeatherBaseUrl"];
    var client = new HttpClient { BaseAddress = new Uri(baseUrl!) };
    return client;
}).As<HttpClient>().Named<HttpClient>("WeatherClient").InstancePerLifetimeScope();

containerBuilder.Register(c =>
{
    var config = c.Resolve<IConfiguration>();
    var baseUrl = config["ApiSettings:NewsBaseUrl"];
    var client = new HttpClient { BaseAddress = new Uri(baseUrl!) };
    return client;
}).As<HttpClient>().Named<HttpClient>("NewsClient").InstancePerLifetimeScope();

containerBuilder.Register(c =>
{
    var config = c.Resolve<IConfiguration>();
    var baseUrl = config["ApiSettings:PlacesApiBaseUrl"];
    var client = new HttpClient { BaseAddress = new Uri(baseUrl!) };
    return client;
}).As<HttpClient>().Named<HttpClient>("PlacesClient").InstancePerLifetimeScope();

containerBuilder.RegisterType<GeoapifyService>()
    .As<IGeoapifyService>()
    .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient), (pi, ctx) => ctx.ResolveNamed<HttpClient>("GeoapifyClient"))
    .InstancePerLifetimeScope();

containerBuilder.RegisterType<WeatherService>()
    .As<IWeatherService>()
    .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient), (pi, ctx) => ctx.ResolveNamed<HttpClient>("WeatherClient"))
    .InstancePerLifetimeScope();

containerBuilder.RegisterType<NewsService>()
    .As<INewsService>()
    .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient), (pi, ctx) => ctx.ResolveNamed<HttpClient>("NewsClient"))
    .InstancePerLifetimeScope();

containerBuilder.RegisterType<PlacesService>()
    .As<IPlacesService>()
    .WithParameter((pi, ctx) => pi.ParameterType == typeof(HttpClient), (pi, ctx) => ctx.ResolveNamed<HttpClient>("PlacesClient"))
    .InstancePerLifetimeScope();

containerBuilder.RegisterType<AggregatorService>()
    .As<IAggregatorService>()
    .InstancePerLifetimeScope();


containerBuilder.Populate(builder.Services);
var container = containerBuilder.Build();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(cb => cb.Populate(builder.Services));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();