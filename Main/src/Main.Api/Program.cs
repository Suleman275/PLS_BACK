using DotNetEnv;
using FastEndpoints;
using FastEndpoints.Security;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using UserManagement.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();
builder.Services.AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["Jwt:SigningKey"]);
builder.Services.AddAuthorization();
builder.Services.AddFastEndpoints();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c => {
    c.Endpoints.RoutePrefix = "api";
    c.Versioning.DefaultVersion = 1;
    c.Versioning.PrependToRoute = true;
    c.Versioning.Prefix = "v";
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});
app.Run();