using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Promos.Resource.Data;
using Promos.Resource.Repositories;

namespace Promos.Resource;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnectionString")));
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder
                        .AllowCredentials()
                        .WithOrigins(
                            "https://localhost:4200")
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
        
        var guestPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim("scope", "dataEventRecords")
            .Build();
        
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
        
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Note: the validation handler uses OpenID Connect discovery
                // to retrieve the address of the introspection endpoint.
                options.SetIssuer("https://localhost:44395/");
                options.AddAudiences("rs_dataEventRecordsApi");

                // Configure the validation handler to use introspection and register the client
                // credentials used when communicating with the remote introspection endpoint.
                options.UseIntrospection()
                    .SetClientId("rs_dataEventRecordsApi")
                    .SetClientSecret("dataEventRecordsSecret");

                // Register the System.Net.Http integration.
                options.UseSystemNetHttp();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });
        
        services.AddScoped<IAuthorizationHandler, RequireScopeHandler>();
        
        services.AddAuthorization(options =>
        {
            options.AddPolicy("dataEventRecordsPolicy", policyUser =>
            {
                policyUser.Requirements.Add(new RequireScope());
            });
        });
        
        services.AddSwaggerGen(c =>
        {
            // add JWT Authentication
            //var securityScheme = new OpenApiSecurityScheme
            //{
            //    Name = "JWT Authentication",
            //    Description = "Enter JWT Bearer token **_only_**",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.Http,
            //    Scheme = "bearer", // must be lower case
            //    BearerFormat = "JWT",
            //    Reference = new OpenApiReference
            //    {
            //        Id = JwtBearerDefaults.AuthenticationScheme,
            //        Type = ReferenceType.SecurityScheme
            //    }
            //};
            //c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            //c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {securityScheme, new string[] { }}
            //});

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Resource server",
                Version = "v1",
                Description = "Recource Server",
                Contact = new OpenApiContact
                {
                    Name = "damienbod",
                    Email = string.Empty,
                    Url = new Uri("https://damienbod.com/"),
                },
            });
        });
        
        services.AddControllers()
            .AddNewtonsoftJson();

        services.AddScoped<DataEventRecordRepository>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resource Server");
            c.RoutePrefix = string.Empty;
        });
        
        app.UseExceptionHandler("/Home/Error");
        app.UseCors("AllowAllOrigins");
        app.UseStaticFiles();
        
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}