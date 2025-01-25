﻿using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.Extensions.Options;
using FIAPX.Auth.Model;
using FIAPX.Auth.Helpers;

namespace FIAPX.Auth.Service
{
    public class CognitoService : ICognitoService
    {
        private readonly OptionsDto _awsOptions;
        private readonly IAmazonCognitoIdentityProvider _client;
        private readonly IAmazonCognitoIdentityProvider _provider;

        public CognitoService(IOptions<OptionsDto> awsOptions, IAmazonCognitoIdentityProvider client, IAmazonCognitoIdentityProvider provider)
        {
            _awsOptions = awsOptions.Value;
            _client = client;
            _provider = provider;
        }
        public CognitoService(IOptions<OptionsDto> awsOptions)
        {
            ArgumentNullException.ThrowIfNull(awsOptions);
            _awsOptions = awsOptions.Value;

            _provider = new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(_awsOptions.Region));
            _client = new AmazonCognitoIdentityProviderClient();
        }
       
        public async Task<ResultadoDto> SignUp(UsuarioDto user)
        {
            if (await UsuarioJaExiste(user))
            {
                return ResultadoDto.Falha("Usuário já cadastrado. Por favor tente autenticar.");
            }

            try
            {
                var secretHash = CognitoHelper.GenerateSecretHash(user.Email, _awsOptions.UserPoolClientId, _awsOptions.ClientPoolSecret);

                var input = new SignUpRequest
                {
                    ClientId = _awsOptions.UserPoolClientId,
                    Username = user.Email,
                    Password = user.Password,
                    SecretHash = secretHash,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "name", Value = user.Nome },
                        new AttributeType { Name = "email", Value = user.Email }
                    }
                };

                var signUpResponse = await _client.SignUpAsync(input);

                if (signUpResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    return ResultadoDto.Falha("Houve algo de errado ao cadastrar o usuário.");

                var confirmRequest = new AdminConfirmSignUpRequest
                {
                    Username = user.Email,
                    UserPoolId = _awsOptions.UserPoolId
                };

                return await UsuarioConfirmado(confirmRequest);

            }
            catch (NotAuthorizedException)
            {
                return ResultadoDto.Falha<TokenDto>("Usuário não autorizado para cadastro com os dados informadoss.");
            }
        }

        public async Task<ResultadoDto<TokenDto>> SignIn(string userName, string userPass)
        {
            try
            {
                using var provider = _provider;
                var secretHash = CognitoHelper.GenerateSecretHash(userName, _awsOptions.UserPoolClientId, _awsOptions.ClientPoolSecret);

                var authRequest = new AdminInitiateAuthRequest
                {
                    UserPoolId = _awsOptions.UserPoolId,
                    ClientId = _awsOptions.UserPoolClientId,
                    AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", userName },
                        { "PASSWORD", userPass },
                        { "SECRET_HASH", secretHash }
                    }
                };

                var authResponse = await _client.AdminInitiateAuthAsync(authRequest);

                if (authResponse.AuthenticationResult != null)
                    return ResultadoDto.Ok(new TokenDto(authResponse.AuthenticationResult.IdToken, authResponse.AuthenticationResult.AccessToken));

                return ResultadoDto.Falha<TokenDto>("Ocorreu um erro ao fazer loginn.");

            }
            catch (UserNotConfirmedException)
            {
                return ResultadoDto.Falha<TokenDto>("Usuário não confirmado.");
            }
            catch (UserNotFoundException)
            {
                return ResultadoDto.Falha<TokenDto>("Usuário não encontrado com os dados informados.");
            }
            catch (NotAuthorizedException)
            {
                return ResultadoDto.Falha<TokenDto>("Usuário não autorizado com os dados informados.");
            }
        }

        private async Task<bool> UsuarioJaExiste(UsuarioDto usuario)
        {
            try
            {
                var adminUser = new AdminGetUserRequest()
                {
                    Username = usuario.Email,
                    UserPoolId = _awsOptions!.UserPoolId
                };

                await _client.AdminGetUserAsync(adminUser);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected async Task<ResultadoDto> UsuarioConfirmado(AdminConfirmSignUpRequest confirmRequest)
        {
            try
            {
                await _client.AdminConfirmSignUpAsync(confirmRequest);
                return ResultadoDto.Ok();
            }
            catch (NotAuthorizedException)
            {
                return ResultadoDto.Falha("Não foi possível confirmar o usuário.");
            }
        }
    }
}
