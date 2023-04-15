using MongoWeatherAPI.Repository;
using MongoWeatherAPI.Repository.Interfaces;
using MongoWeatherAPI.Services;
using MongoWeatherAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MongoWeatherAPI.xml"));
});


builder.Services.Configure<MongoDbConnectionSettings>(builder.Configuration.GetSection("MongoDbConnectionSettings"));
builder.Services.AddScoped<MongoConnection>();

builder.Services.AddCors(c => c.AddPolicy("GooglePolicy", d =>
{
    d.WithOrigins(builder.Configuration.GetSection("CorsUrls").GetValue<string>("Google.com"),
    builder.Configuration.GetSection("CorsUrls").GetValue<string>("Google.com.au"));
    //d.WithOrigins("https://www.google.com", "https://www.google.com.au");
    d.AllowAnyHeader();
    d.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH");
}));

builder.Services.AddScoped<IWeatherDataRepository, MongoWeatherDataRepository>();
builder.Services.AddScoped<IApiUserRepository, MongoApiUserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
