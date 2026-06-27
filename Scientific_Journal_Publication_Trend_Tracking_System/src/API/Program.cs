using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Handlers;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Validators;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Validators;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Validators;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Validators;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Middleware;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.Authentication;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.BackgroundJobs;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.Seeding;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Shared.Behaviors;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            //Research Paper Repository
            builder.Services.AddScoped<IResearchPaperRepository, ResearchPaperRepository>();
            builder.Services.AddScoped<ITrendAnalyticsRepository, TrendAnalyticsRepository>();

            builder.Services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationBehavior<,>));

            // ── JWT Authentication + Authorization (extracted to extension method) ──
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            //Add Hangfire
            builder.Services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(options =>
                        options.UseNpgsqlConnection(
                            builder.Configuration.GetConnectionString("DefaultConnection")));
            });

            builder.Services.AddHangfireServer();

            builder.Services.AddScoped<SyncResearchPapersJob>();


            // Add MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommandHandler).Assembly));

            // Add FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<SearchPapersRequestValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<GetKeywordTrendQueryValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<GetPublicationsByYearChartQueryValidator>();

            // Add AutoMapper
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Add External API Services
            builder.Services.AddHttpClient<IExternalApiSyncService, ExternalApiSyncService>()
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Scientific-Journal-Tracking-System/1.0");
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

            // Add Authentication Services
            builder.Services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddHttpContextAccessor();

            // Add Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger/OpenAPI with JWT Bearer Token Authentication
            builder.Services.AddSwaggerGen(options =>
            {
                // Add JWT Bearer Token security definition
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT Bearer token in the text input below.\n\nExample: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
                });

                // Add security requirement to operations
                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            var app = builder.Build();

            //Configure Hangfire
            app.UseHangfireDashboard();

            RecurringJob.AddOrUpdate<SyncResearchPapersJob>(
                        "weekly-paper-sync",
                        job => job.Execute(),
                        Cron.Weekly);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Scientific Journal API v1");
                    options.RoutePrefix = string.Empty;
                    options.DefaultModelsExpandDepth(0);
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    options.EnablePersistAuthorization(); // keeps the Bearer token across page reloads/restarts
                });
            }

            // Use Global Exception Handling Middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Only force HTTPS redirection outside Development.
            // (See note above: redirecting http→https on localhost crosses a
            // port/origin boundary, and browsers strip the Authorization
            // header on cross-origin redirects — this was silently breaking
            // Bearer auth when testing via Swagger over http://localhost:5220.)
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Use CORS
            app.UseCors("AllowAll");

            // Use Routing
            app.UseRouting();

            //
            await AdminSeeder.SeedAdminAsync(app.Services);
            await ResearchTopicSeeder.SeedAsync(app.Services);

            // Use Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}