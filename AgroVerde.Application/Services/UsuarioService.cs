using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
    }

    public async Task<AppResponse<UsuarioResponse>> RegistrarAsync(
        RegistrarUsuarioRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                return AppResponse<UsuarioResponse>.Fail(
                    "Informe o nome."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            {
                return AppResponse<UsuarioResponse>.Fail(
                    "Informe um e-mail válido."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            {
                return AppResponse<UsuarioResponse>.Fail(
                    "A senha deve ter pelo menos 6 caracteres."
                );
            }

            var email = request.Email.Trim().ToLowerInvariant();

            if (await _usuarioRepository.EmailExisteAsync(email))
            {
                return AppResponse<UsuarioResponse>.Fail(
                    "O e-mail informado já está em uso."
                );
            }

            var usuario = new Usuario
            {
                Nome = request.Nome.Trim(),
                Email = email,
                SenhaHash = SenhaHasher.Hash(request.Senha)
            };

            var usuarioCriado = await _usuarioRepository.InserirAsync(usuario);

            return AppResponse<UsuarioResponse>.Ok(
                "Usuário cadastrado com sucesso.",
                ParaResponse(usuarioCriado)
            );
        }
        catch (Exception ex)
        {
            return AppResponse<UsuarioResponse>.Fail(
                "Erro ao cadastrar o usuário.",
                ex
            );
        }
    }

    public async Task<AppResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _usuarioRepository.BuscarPorEmailAsync(email);

            if (usuario == null ||
                !SenhaHasher.Verificar(request.Senha ?? string.Empty, usuario.SenhaHash))
            {
                return AppResponse<LoginResponse>.Fail(
                    "E-mail ou senha inválidos."
                );
            }

            var response = new LoginResponse
            {
                Token = _tokenService.GerarToken(usuario),
                Usuario = ParaResponse(usuario)
            };

            return AppResponse<LoginResponse>.Ok(
                "Login realizado com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<LoginResponse>.Fail(
                "Erro ao realizar o login.",
                ex
            );
        }
    }

    public async Task<AppResponse<bool>> AlterarSenhaAsync(
        int usuarioId,
        AlterarSenhaRequest request)
    {
        try
        {
            var usuario = await _usuarioRepository.BuscarPorIdAsync(usuarioId);

            if (usuario == null)
            {
                return AppResponse<bool>.Fail(
                    "Usuário não encontrado.",
                    new List<string> { "Usuário não encontrado." }
                );
            }

            // Senha atual conferida com o mesmo PBKDF2 usado no login.
            if (string.IsNullOrEmpty(request.SenhaAtual)
                || !SenhaHasher.Verificar(request.SenhaAtual, usuario.SenhaHash))
            {
                return AppResponse<bool>.Fail(
                    "A senha atual está incorreta.",
                    new List<string> { "A senha atual está incorreta." }
                );
            }

            if (string.IsNullOrWhiteSpace(request.NovaSenha) || request.NovaSenha.Length < 6)
            {
                return AppResponse<bool>.Fail(
                    "A nova senha deve ter pelo menos 6 caracteres.",
                    new List<string> { "A nova senha deve ter pelo menos 6 caracteres." }
                );
            }

            if (request.NovaSenha == request.SenhaAtual)
            {
                return AppResponse<bool>.Fail(
                    "A nova senha deve ser diferente da senha atual.",
                    new List<string> { "A nova senha deve ser diferente da senha atual." }
                );
            }

            usuario.SenhaHash = SenhaHasher.Hash(request.NovaSenha);

            await _usuarioRepository.AtualizarAsync(usuario);

            return AppResponse<bool>.Ok("Senha alterada com sucesso.", true);
        }
        catch (Exception ex)
        {
            return AppResponse<bool>.Fail("Erro ao alterar a senha.", ex);
        }
    }

    private static UsuarioResponse ParaResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email
        };
    }
}
