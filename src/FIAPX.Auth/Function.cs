using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FIAPX.Auth.Helpers;
using FIAPX.Auth.Model;
using FIAPX.Auth.Service;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FIAPX.Auth;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Post, "/auth")]
    public async Task<APIGatewayProxyResponse> LambdaAuth(APIGatewayProxyRequest request,
                                                  ILambdaContext context,
                                                  [FromServices] ICognitoService cognitoService,
                                                  [FromServices] IOptions<OptionsDto> awsOptions)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(awsOptions);

            var usuario = ObterUsuario(request, awsOptions.Value);           

            var resultadoLogin = await cognitoService.SignIn(usuario.Email, usuario.Password);
            if (!resultadoLogin.Sucesso)
            {
                return Response.BadRequest(resultadoLogin.Mensagem);
            }

            var tokenResult = resultadoLogin.Value!;
            return !string.IsNullOrEmpty(tokenResult.AccessToken) ? Response.Ok(tokenResult) : Response.BadRequest("Não possui token");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Post, "/signup")]
    public async Task<APIGatewayProxyResponse> LambdaSignUP(APIGatewayProxyRequest request,
                                                  ILambdaContext context,
                                                  [FromServices] ICognitoService cognitoService,
                                                  [FromServices] IOptions<OptionsDto> awsOptions)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(awsOptions);

            var usuario = ObterUsuario(request, awsOptions.Value);
            
            var resultadoCadastroUsuario = await cognitoService.SignUp(usuario);
            if (!resultadoCadastroUsuario.Sucesso)
            {
                return Response.BadRequest(resultadoCadastroUsuario.Mensagem);
            }

            var resultadoLogin = await cognitoService.SignIn(usuario.Email, usuario.Password);
            if (!resultadoLogin.Sucesso)
            {
                return Response.BadRequest(resultadoLogin.Mensagem);
            }

            var tokenResult = resultadoLogin.Value;
            return !string.IsNullOrEmpty(tokenResult.AccessToken) ? Response.Ok(tokenResult) : Response.BadRequest("Não possui token");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private UsuarioDto ObterUsuario(APIGatewayProxyRequest request, OptionsDto awsOptions)
    {
        var usuario = JsonConvert.DeserializeObject<UsuarioDto>(request.Body) ?? new UsuarioDto();
        ArgumentNullException.ThrowIfNull(usuario);
        var user = new UsuarioDto(usuario.Password, usuario.Email, usuario.Nome);
        return user;
    }
}
