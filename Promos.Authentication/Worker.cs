using System.Globalization;
using OpenIddict.Abstractions;
using Promos.Authentication.Data;

namespace Promos.Authentication;

 public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        await RegisterApplicationsAsync(scope.ServiceProvider);
        await RegisterScopesAsync(scope.ServiceProvider);

        static async Task RegisterApplicationsAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

            // Dashboard UI client
            if (await manager.FindByClientIdAsync("dashboardclient") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "dashboardclient",
                    ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                    DisplayName = "dashboard client PKCE",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("en-US")] = "Application client MVC"
                    },
                    PostLogoutRedirectUris =
                    {
                        new Uri("https://localhost:5000")
                    },
                    RedirectUris =
                    {
                        new Uri("https://localhost:5000/auth-callback")
                    },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Logout,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Prefixes.Scope + "barcodeRecords"
                    },
                    Requirements =
                    {
                        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            // API
            if (await manager.FindByClientIdAsync("oid_externalApi") == null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "oid_externalApi",
                    ClientSecret = "externalApiSecret",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Introspection
                    }
                };

                await manager.CreateAsync(descriptor);
            }
        }

        static async Task RegisterScopesAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByNameAsync("barcodeRecords") is null)
            {
                await manager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    DisplayName = "external API access",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("en-US")] = "Access external api"
                    },
                    Name = "barcodeRecords",
                    Resources =
                    {
                        "oid_externalApi"
                    }
                });
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}