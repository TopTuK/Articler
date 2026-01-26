

using Articler.AppDomain.Constants;
using Articler.WebApi.Middlewares;
using Articler.WebApi.Services.Auth;
using Articler.WebApi.Services.Chat;
using Articler.WebApi.Services.DataSource;
using Articler.WebApi.Services.Project;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

static void ConfigureServices(IServiceCollection services)
{
    // Singleton
    services.AddSingleton<IJwtSecurityService, JwtSecurityService>();

    // Scoped
    services.AddScoped<UserAuthMiddleware>();
    services.AddScoped<UserIdMiddleware>();
    services.AddScoped<CheckUserMiddleware>();

    services.AddScoped<IAuthenticationService, AuthenticationService>();
    services.AddScoped<IProjectService, ProjectService>();
    services.AddScoped<IChatService, ChatService>();
    services.AddScoped<IDocumentService, DocumentService>();

    // Transient
    services.AddTransient<CheckUserPipeline>();
}

static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
{

}

// Configure logger
// https://github.com/serilog/serilog-aspnetcore
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    /* ADD AUTHENTICATION */
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(configuration["Auth:AuthDefaultSchemeName"]!, options =>
        {
            options.Cookie.Name = configuration["Auth:AuthDefaultSchemeName"];
            options.ExpireTimeSpan = TimeSpan.FromDays(7.0);

            // TODO: change this
            options.Cookie.HttpOnly = false;

            options.LoginPath = new PathString("/login");
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;

            // https://metanit.com/sharp/aspnet6/13.2.php
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate publisher (issuer) of token
                ValidateIssuer = true,
                ValidIssuer = configuration["Auth:Jwt:ValidIssuer"],

                // Validate consumer (audience) of token
                ValidateAudience = true,
                ValidAudience = configuration["Auth:Jwt:ValidAudience"],

                // Validate lifetime of token
                ValidateLifetime = true,

                // Validate signature key
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Auth:Jwt:Key"]!)),
                ValidateIssuerSigningKey = true,
            };
        })
        .AddCookie(configuration["Auth:TempCookieName"]!)
        .AddOpenIdConnect(configuration["Auth:Oidc:Name"]!, options =>
        {
            options.Authority = configuration["Auth:Oidc:Authority"]!;

            options.ClientId = configuration["Auth:Oidc:ClientId"];
            options.ClientSecret = configuration["Auth:Oidc:ClientSecret"];

            // Set the callback path, so it will call back to.
            options.CallbackPath = new PathString(configuration["Auth:Oidc:CallbackUrl"]);

            // Set response type to code
            options.ResponseType = OpenIdConnectResponseType.Code;

            // Configure the scope
            options.Scope.Clear();
            options.Scope.Add("openid");

            // save tokens
            options.SaveTokens = true;

            options.Events.OnAuthorizationCodeReceived = async (context) =>
            {
                //var request = context.HttpContext.Request;
                var redirectUri = context
                    .Properties
                    ?.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey] ?? "/";
                var code = context.ProtocolMessage.Code;

                using var client = new HttpClient();
                var discoResponsee = await client.GetDiscoveryDocumentAsync(options.Authority);

                var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(new ()
                {
                    Address = discoResponsee.TokenEndpoint,
                    ClientId = options.ClientId!,
                    ClientSecret = options.ClientSecret,
                    Code = code,
                    RedirectUri = redirectUri,
                });

                if (tokenResponse.IsError)
                {
                    // Error handler
                    throw new Exception("OpenIdConnect::Bad auth. Can't exchange code for access token and id token");
                }

                var accessToken = tokenResponse.AccessToken ?? string.Empty;
                var idToken = tokenResponse.IdentityToken ?? string.Empty;

                context.HandleCodeRedemption(accessToken, idToken);
            };

            options.MapInboundClaims = false;
            options.SignInScheme = configuration["Auth:TempCookieName"]!;
        });

    /* Add polices */
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Free", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("AccountType", "Free");
        });

        options.AddPolicy("Trial", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("AccountType", "Trial");
        });

        options.AddPolicy("Paid", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("AccountType", "Paid");
        });

        options.AddPolicy("Super", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("AccountType", "Super");
        });
    });

    // Configure options
    ConfigureOptions(builder.Services, configuration);

    // Configure application services
    ConfigureServices(builder.Services);

    var environment = builder.Environment;

    // Add Orleans client
    builder.UseOrleansClient(client =>
    {
        if (environment.IsDevelopment())
        {
            // Use localhost clustering for local development
            client.UseLocalhostClustering();
        }

        // Configure connection retry for better reliability
        client.Configure<Orleans.Configuration.ClusterOptions>(options =>
        {
            options.ClusterId = OrleansConstants.ClusterId;
            options.ServiceId = OrleansConstants.ServiceId;
        });

        // Add retry logic for gateway connections
        client.Configure<Orleans.Configuration.GatewayOptions>(options =>
        {
            options.GatewayListRefreshPeriod = TimeSpan.FromSeconds(OrleansConstants.GatewayListRefreshPeriod);
        });
    });

    // Add controllers to the container
    builder.Services
        .AddControllersWithViews();

    /* BUILD */
    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios
        // see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseRouting();

    // https://habr.com/ru/articles/468401/
    app.UseMiddleware<UserAuthMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<UserIdMiddleware>();

#pragma warning disable ASP0014 // Suggest using top level route registrations
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "api/{controller}/{action=Index}/{id?}"
        );
    });
#pragma warning restore ASP0014 // Suggest using top level route registrations

    if (app.Environment.IsDevelopment())
    {
        app.UseSpa(spa =>
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
        });
    }
    else
    {
        app.MapFallbackToFile("index.html");
    }

    /* RUN APP */
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Articler.WebAPI application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}