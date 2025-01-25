using FIAPX.Auth.Model;

namespace FIAPX.Auth.Service
{
    public interface ICognitoService
    {
        Task<ResultadoDto> SignUp(UsuarioDto usuario);
        Task<ResultadoDto<TokenDto>> SignIn(string userName, string userPass);
    }
}
