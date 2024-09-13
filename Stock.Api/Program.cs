using Stock.Application.Interfaces;
using Stock.Application.Services;
using Stock.Domain.Interfaces;
using Stock.Infrastructure.Data;
using Stock.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Stock.Application.Commands;
using FluentValidation.AspNetCore;
using Stock.Application.Validators;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

Env.Load();

builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateStockCommand).Assembly));

builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<CreateStockCommandValidator>();
    fv.RegisterValidatorsFromAssemblyContaining<UpdateStockCommandValidator>();
});


builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();