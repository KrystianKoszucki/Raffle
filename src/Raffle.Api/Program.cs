using Microsoft.EntityFrameworkCore;
using Raffle.Api.Database;
using Raffle.Api.Middleware;
using Raffle.Api.Services;
using Raffle.Api.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateRaffleDrawRequestValidator).Assembly);

builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IRaffleService, RaffleService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<RaffleDbContext>(options =>
    options.UseSqlite("Data Source=raffle.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Raffle API V1");
        c.RoutePrefix = "";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
