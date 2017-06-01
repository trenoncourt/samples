using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIdFlows.ClientCredentialsGrantFlow.Data;

namespace OpenIdFlows.ClientCredentialsGrantFlow
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore();
            services.AddDirectoryBrowser();
            services.AddAuthentication();
            services.AddSingleton<ApplicationRepository>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationRepository applicationRepository)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole();
            }



            app.UseOpenIdConnectServer(options =>
            {
                options.TokenEndpointPath = "/connect/token";

                // Active Ephemeral key and http for development.
                if (env.IsDevelopment())
                {

                    options.SigningCredentials.AddEphemeralKey();
                    options.AllowInsecureHttp = true;
                }

                // Use a cert in others environments.
                if (env.IsEnvironment("...") || env.IsProduction())
                {
                    options.SigningCredentials.AddCertificate(
                        assembly: typeof(Startup).GetTypeInfo().Assembly,
                        resource: "foo.pfx",
                        password: "Bar");
                }

                options.AccessTokenHandler = new JwtSecurityTokenHandler();

                // Implement OnValidateTokenRequest to support flows using the token endpoint.
                options.Provider.OnValidateTokenRequest = context =>
                {
                    if (context.Request.IsClientCredentialsGrantType())
                    {
                        Application application = applicationRepository.FindByClientId(context.Request.ClientId);

                        if (application == null)
                        {
                            context.Reject(error: OpenIdConnectConstants.Errors.UnauthorizedClient,
                                description: "Invalid client_id or client_secret.");
                            return Task.FromResult(0);
                        }

                        // Use Hash.
                        bool isClientSecretVeriied = application.ClientSecret == context.Request.ClientSecret;
                        if (!isClientSecretVeriied)
                        {
                            context.Reject(error: OpenIdConnectConstants.Errors.UnauthorizedClient,
                                description: "Invalid client_id or client_secret.");
                            return Task.FromResult(0);
                        }

                        context.Validate();
                        return Task.FromResult(0);

                    }

                    context.Skip();
                    return Task.FromResult(0);
                };


                // Implement OnHandleTokenRequest to support token requests.
                options.Provider.OnHandleTokenRequest = context =>
                {

                    if (context.Request.IsClientCredentialsGrantType())
                    {
                        var identity = new ClaimsIdentity(context.Options.AuthenticationScheme, OpenIdConnectConstants.Claims.Name,
                            OpenIdConnectConstants.Claims.Role);

                        // Add the mandatory subject/user identifier claim.
                        identity.AddClaim(OpenIdConnectConstants.Claims.Subject, Guid.NewGuid().ToString());


                        // By default, claims are not serialized in the access/identity tokens.
                        // Use the overload taking a "destinations" parameter to make sure
                        // your claims are correctly inserted in the appropriate tokens.
                        identity.AddClaim(OpenIdConnectConstants.Claims.ClientId, context.Request.ClientId, OpenIdConnectConstants.Destinations.AccessToken);

                        var ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(identity),
                            new AuthenticationProperties(),
                            context.Options.AuthenticationScheme);

                        ticket.SetResources("resource_server_1");

                        context.Validate(ticket);

                    }
                    return Task.FromResult(0);
                };
            });
        }
    }
}
