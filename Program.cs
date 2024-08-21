using APIAggregation.Interfaces;
using APIAggregation.Services;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container.

builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddHttpClient<ITwitterService, TwitterService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

builder.Services.AddScoped<IAggregationService, AggregationService>();

// Add caching services
builder.Services.AddMemoryCache();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
