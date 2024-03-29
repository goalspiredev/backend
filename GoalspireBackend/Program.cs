
using System.Text;
using FluentEmail.Core;
using FluentEmail.MailKitSmtp;
using GoalspireBackend.Common.Swagger;
using GoalspireBackend.Data;
using GoalspireBackend.Models;
using GoalspireBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GoalspireBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "GoalspireCorsPolicy", policy =>
                {
                    policy.WithOrigins("https://goalspire.net", "https://test.goalspire.net", "https://dev.goalspire.net", "https://dev.frontend-429.pages.dev", "https://docs.goalspire.net", "http://localhost:5173", "http://localhost:4173").AllowAnyHeader().AllowAnyMethod();
                });
            });

            builder.Services.AddDbContext<DataContext>();

            //all the stuff about handling users and their logins
            builder.Services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = true;

                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 8;
                    options.Password.RequiredUniqueChars = 3;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JWT:ValidAudience"],
                        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
                    };
                });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
            });

            //seeting up the SMTP service for sending emails
            builder.Services
                .AddFluentEmail("no-reply@goalspire.net", "Goalspire")
                .AddMailKitSender(new SmtpClientOptions
                {
                    Server = builder.Configuration["SMTP:Host"],
                    Port = builder.Configuration.GetValue<int>("SMTP:Port"),
                    User = builder.Configuration["SMTP:Username"],
                    Password = builder.Configuration["SMTP:Password"],
                    UseSsl = true,
                    RequiresAuthentication = true,
                });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<DataContext>();

            builder.Services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            builder.Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
            builder.Services.AddSwaggerGen(c =>
            {
#if DEBUG // reordering the options for servers on swagger, so when debugging the local API is default
                c.AddServer(new OpenApiServer
                {
                    Description = "Local API",
                    Url = "https://localhost:7101/",
                });
                c.AddServer(new OpenApiServer
                {
                    Description = "Testing API",
                    Url = "https://api.test.goalspire.net/",
                });
                c.AddServer(new OpenApiServer
                {
                    Description = "Production API",
                    Url = "https://api.goalspire.net",
                });
#else
                c.AddServer(new OpenApiServer
                {
                    Description = "Production API",
                    Url = "https://api.goalspire.net",
                });

                c.AddServer(new OpenApiServer
                {
                    Description = "Testing API",
                    Url = "https://api.test.goalspire.net/",
                });

                c.AddServer(new OpenApiServer
                {
                    Description = "Local API",
                    Url = "https://localhost:7101/",
                });
#endif

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                var filePath = Path.Combine(AppContext.BaseDirectory, "GoalspireBackend.xml");
                c.IncludeXmlComments(filePath);
            });


            // Registering all the services

            builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<ISettingsService, SettingsService>();

            builder.Services.AddTransient<INotificationService, NotificationService>();

            builder.Services.AddTransient<IGoalsService, GoalsService>();

            builder.Services.AddHostedService<RemindingService>();



            var app = builder.Build();
            
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                if (serviceScope != null)
                {
                    try
                    {
                        var context = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
                        context.Database.Migrate();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed migration: ${e.Message}");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var apiVersionDescriptionProvider = app.Services.GetService<IApiVersionDescriptionProvider>();
                    foreach (var desc in apiVersionDescriptionProvider!.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"API v{desc.ApiVersion}");
                        options.DefaultModelsExpandDepth(-1);
                        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    }
                });
            }

            app.UseCors("GoalspireCorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
