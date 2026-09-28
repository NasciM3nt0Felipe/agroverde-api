using AgroVerde.Application.Services;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using AgroVerde.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Serviços da aplicação
builder.Services.AddScoped<ITalhaoService, TalhaoService>();
builder.Services.AddScoped<ISafraService, SafraService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();

// Repositórios
builder.Services.AddScoped<ITalhaoRepository, TalhaoRepository>();
builder.Services.AddScoped<ISafraRepository, SafraRepository>();
builder.Services.AddScoped<IEstoqueRepository, EstoqueRepository>();

// Banco de dados SQLite
builder.Services.AddDbContext<AgroVerdeDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger disponível em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();