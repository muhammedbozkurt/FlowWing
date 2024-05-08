using System.Configuration;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using FlowWing.API.Helper;
using FlowWing.API.Helpers;
using FlowWing.API.Middlewares;
using FlowWing.Business.Abstract;
using FlowWing.Business.Concrete;
using FlowWing.DataAccess;
using FlowWing.DataAccess.Abstract;
using FlowWing.DataAccess.Concrete;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FlowWing.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("HangfireConnection");
            var databaseConnectionString = Configuration.GetConnectionString("DatabaseConnection");
            

            services.AddDbContext<FlowWingDbContext>(options =>
                       options.UseSqlServer(databaseConnectionString)
                );
            services.AddHangfire(config =>
                config.UseSqlServerStorage(
                    connectionString,
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        UseRecommendedIsolationLevel = true,
                        UsePageLocksOnDequeue = true,
                        DisableGlobalLocks = true
                    }
                )
            );

            services.AddHangfireServer();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });

            services.Configure<AppSettings>(Configuration);
            services.AddScoped<ScheduledMailHelper>();
            services.AddScoped<EmailSenderService>();

            services.AddScoped<IEmailLogRepository, EmailLogRepository>();
            services.AddScoped<IEmailLogService, EmailLogManager>();

            services.AddScoped<IScheduledEmailRepository, ScheduledEmailRepository>();
            services.AddScoped<IScheduledEmailService, ScheduledEmailManager>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserManager>();
            
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IAttachmentService, AttachmentManager>();

            services.AddScoped<ILoggingRepository, LoggingRepository>();
            services.AddScoped<ILoggingService, LoggingManager>();

            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleService, RoleManager>();
            
            services.AddControllers();
            
            // JWT Authentication ekle
            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("SecretKey").Value);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = ClaimTypes.Role // Rol bilgisini burada belirtiyoruz
                };
            });

            // Swagger belgesi ekle
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FlowWing API", 
                    Version = "v1",
                    Description = "FlowWing API",
                    Contact = new OpenApiContact
                    {
                        Name = "Kerem Mert Izmir",
                        Email = "keremmertizmir39@gmail.com"
                        
                    },
                });
                c.IncludeXmlComments(xmlPath);

                // JWT yetkilendirme ekle
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            services.AddHostedService<RoleInitializer>();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UsePathBase("/FlowWingAPI");

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "FlowWing Hangfire Dashboard"
            });
            
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/FlowWingAPI/swagger/v1/swagger.json", "FlowWing API v1");
                
            });
            // app.UseMiddleware<LoggingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseWhen(context =>
                context.Request.Path.StartsWithSegments("/api/EmailLogs") ||
                context.Request.Path.StartsWithSegments("/api/ScheduledEmails"),
                builder =>
                {
                    builder.UseMiddleware<AuthorizationMiddleware>();
                }
            );

            
            app.UseMiddleware<EmailOwnershipMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });   
        }
    }
}
