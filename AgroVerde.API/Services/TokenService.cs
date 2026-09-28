using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgroVerde.Application.Services;
using AgroVerde.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AgroVerde.API.Services;

// Fica na camada API (que já tem o pacote JwtBearer e o IConfiguration), então
// não precisa adicionar pacote novo no Infrastructure. Implementa a abstração
// ITokenService definida no Application.
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(Usuario usuario)
    {
        var jwt = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwt["Key"]!);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nome)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);

        return handler.WriteToken(token);
    }
}
