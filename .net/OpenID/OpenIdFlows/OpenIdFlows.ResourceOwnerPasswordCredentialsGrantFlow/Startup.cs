using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Identity.PasswordHasher;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIdFlows.ResourceOwnerPasswordCredentialsGrantFlow.Data;

namespace OpenIdFlows.ResourceOwnerPasswordCredentialsGrantFlow
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore();
            services.AddDirectoryBrowser();
            services.AddAuthentication();
            services
                .AddSingleton<UserRepository>()
                .AddSingleton<PasswordHasher>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, UserRepository userRepository, PasswordHasher passwordHasher)
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
                    // Reject token requests that don't use grant_type=password or grant_type=refresh_token.
                    if (!context.Request.IsPasswordGrantType()
                        && !context.Request.IsRefreshTokenGrantType())
                    {
                        context.Reject(
                            error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                            description: "Only grant_type=password and refresh_token " +
                                         "requests are accepted by this server.");

                        return Task.FromResult(0);
                    }

                    context.Skip();
                    return Task.FromResult(0);
                };


                // Implement OnHandleTokenRequest to support token requests.
                options.Provider.OnHandleTokenRequest = context =>
                {
                    // Only handle grant_type=password.
                    if (context.Request.IsPasswordGrantType())
                    {
                        User user = userRepository.FindByEmail(context.Request.Username);
                        if (user == null)
                        {
                            context.Reject(
                                error: OpenIdConnectConstants.Errors.UnauthorizedClient,
                                description: "Invalid email.");

                            return Task.FromResult(0);
                        }

                        bool isPasswordVerified = passwordHasher.VerifyHashedPassword(user.Password, context.Request.Password);
                        if (!isPasswordVerified)
                        {
                            context.Reject(
                                error: OpenIdConnectConstants.Errors.InvalidGrant,
                                description: "Invalid user credentials.");

                            return Task.FromResult(0);
                        }

                        var identity = new ClaimsIdentity(context.Options.AuthenticationScheme, OpenIdConnectConstants.Claims.Name, OpenIdConnectConstants.Claims.Role);

                        // Add the mandatory subject/user identifier claim.
                        identity.AddClaim(OpenIdConnectConstants.Claims.Subject, Guid.NewGuid().ToString());

                        // By default, claims are not serialized in the access/identity tokens.
                        // Use the overload taking a "destinations" parameter to make sure
                        // your claims are correctly inserted in the appropriate tokens.
                        identity.AddClaim(ClaimTypes.Sid, user.Id.ToString(), OpenIdConnectConstants.Destinations.AccessToken);

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
