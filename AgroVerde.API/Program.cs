using System.Text;
using AgroVerde.API.Services;
using AgroVerde.Application.Services;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using AgroVerde.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Serviços da aplicação
builder.Services.AddScoped<ITalhaoService, TalhaoService>();
builder.Services.AddScoped<ISafraService, SafraService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPropriedadeService, PropriedadeService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Repositórios
builder.Services.AddScoped<ITalhaoRepository, TalhaoRepository>();
builder.Services.AddScoped<ISafraRepository, SafraRepository>();
builder.Services.AddScoped<IEstoqueRepository, EstoqueRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPropriedadeRepository, PropriedadeRepository>();

// Banco de dados SQLite
builder.Services.AddDbContext<AgroVerdeDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Cache em memória
builder.Services.AddMemoryCache();

// Autenticação JWT
var jwt = builder.Configuration.GetSection("Jwt");
var chave = Encoding.ASCII.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(chave),
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Swagger / OpenAPI (com suporte a token Bearer)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole apenas o token JWT (sem a palavra Bearer)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Swagger disponível em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication SEMPRE antes de Authorization, e os dois antes do MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();