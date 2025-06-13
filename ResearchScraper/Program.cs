using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;
using StackExchange.Redis;
using ResearchScrapper.Api.Extensions;
using ResearchScrapper.Api.MiddleWare;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200")

            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    options.AddPolicy("ProdCors", policy =>
    {
        policy.WithOrigins("https://articulio.netlify.app")
        .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();

    });
});

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var mongoDbConnection = config["MongoDbConnection"];

    return new MongoClient(mongoDbConnection);
});


builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddOAuthProviders(builder.Configuration);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<MediumScraperService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IArticleCacheService, RedisBackedCacheService>();
builder.Services.AddHttpClient<CoreApiService>();
builder.Services.AddScoped<NewsApiScraperService>();
builder.Services.AddScoped<DevToScraperService>();
builder.Services.AddScoped<CrossrefScientificArticleService>();
builder.Services.AddScoped<ArxivArticleService>();
builder.Services.AddScoped<PubMedArticleService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<IAggregateService, AggregateScraperService>();
builder.Services.AddScoped<IAggregateScientificPapersService, AggregateScientificPapersService>();
builder.Services.AddSingleton<MetaScraper>();
builder.Services.AddSingleton<IQuerySanitizationService, QuerySanitizationService>();
builder.Services.AddHttpClient<IScientificArticleService, CrossrefScientificArticleService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddHealthChecks();

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

    var config = new ConfigurationOptions
    {
        EndPoints = { { "redis-12757.c328.europe-west3-1.gce.redns.redis-cloud.com", 12757 } },
        User = options.User,
        Password = options.Password,
        Ssl = false
    };

    var connection = ConnectionMultiplexer.Connect(config);

    connection.ConnectionFailed += (sender, args) =>
    {
        Console.WriteLine($"Redis connection failed: {args.Exception?.Message}");
    };

    connection.ConnectionRestored += (sender, args) =>
    {
        Console.WriteLine("Redis connection restored");
    };

    connection.ConfigurationChanged += (sender, args) =>
    {
        Console.WriteLine("Redis configuration changed");
    };

    connection.ErrorMessage += (sender, args) =>
    {
        Console.WriteLine($"Redis error: {args.Message}");
    };

    connection.InternalError += (sender, args) =>
    {
        Console.WriteLine($"Redis internal error: {args.Exception?.Message}");
    };

    Console.WriteLine("Redis connection established");

    return connection;
});

builder.Services.AddSingleton<IIpAbuseService, RedisIpAbuseService>();
builder.Services.AddSingleton<ArticleViewTrackerService>();
builder.Services.AddSingleton<ScientificArticleDownloadTrackerService>();
builder.Services.AddSingleton<ScientificArticleDoiTrackerService>();
builder.Services.AddScoped<IFavoriteArticleRepository, FavoriteArticleRepository>();
builder.Services.AddScoped<IFavoriteScientificArticleRepository, FavoriteScientificArticleRepository>();
builder.Services.AddScoped<IToReadScientificArticleRepository, ToReadScientificArticleRepository>();
builder.Services.AddScoped<IToReadArticleRepository, ToReadArticleRepository>();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Research Scrapper API",
        Version = "v1"
    });
});

builder.Services.AddHostedService<ArticleViewRecalculationService>();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Research Scrapper API V1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}
else
{
    app.UseCors("ProdCors");
}

app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();



