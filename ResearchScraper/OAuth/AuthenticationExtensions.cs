using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ResearchScrapper.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthProviders(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
})
            .AddGoogle("Google", options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey("picture", "picture");
            })
            .AddOAuth("Discord", options =>
            {
                options.ClientId = configuration["Authentication:Discord:ClientId"];
                options.ClientSecret = configuration["Authentication:Discord:ClientSecret"];
                options.CallbackPath = "/signin-discord";

                options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
                options.TokenEndpoint = "https://discord.com/api/oauth2/token";
                options.UserInformationEndpoint = "https://discord.com/api/users/@me";

                options.Scope.Add("identify");
                options.Scope.Add("email");

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        using var response = await context.Backchannel.SendAsync(request);
                        response.EnsureSuccessStatusCode();

                        using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        var root = user.RootElement;

                        var userId = root.GetProperty("id").GetString();
                        var avatar = root.GetProperty("avatar").GetString();
                        var avatarUrl = $"https://cdn.discordapp.com/avatars/{userId}/{avatar}.png";

                        context.Identity.AddClaim(new Claim("picture", avatarUrl));
                        context.RunClaimActions(root);
                    }
                };
            });

            return services;
        }
    }
}

