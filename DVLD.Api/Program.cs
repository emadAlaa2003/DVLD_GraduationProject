using DVLD.Api.Authentication;
using DVLD.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<
    IKnowledgeDocumentProcessingQueue,
    KnowledgeDocumentProcessingQueue>();

builder.Services.AddHostedService<
    KnowledgeDocumentBackgroundWorker>();

builder.Services.AddSingleton<
    IQuestionGenerationProcessingQueue,
    QuestionGenerationProcessingQueue>();

builder.Services.AddHostedService<
    QuestionGenerationBackgroundWorker>();

builder.Services.AddOpenApi();

// Centralized API error responses
builder.Services.AddProblemDetails();

// Mobile citizen authentication session duration.
int mobileSessionHours =
    builder.Configuration.GetValue<int?>(
        "MobileAuth:SessionHours") ?? 8;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            MobileAuthDefaults.Scheme;

        options.DefaultChallengeScheme =
            MobileAuthDefaults.Scheme;
    })
    .AddCookie(
        MobileAuthDefaults.Scheme,
        options =>
        {
            options.Cookie.Name =
                MobileAuthDefaults.CookieName;

            options.Cookie.HttpOnly = true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Strict;

            options.ExpireTimeSpan =
                TimeSpan.FromHours(mobileSessionHours);

            options.SlidingExpiration = false;

            options.Events =
                new MobileCookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        return Task.CompletedTask;
                    },

                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        return Task.CompletedTask;
                    }
                };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        MobileAuthDefaults.EmployeeApiPolicy,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            // Employee API authentication is intentionally
            // out of scope for this prototype.
            // This keeps administrative people endpoints closed
            // to both visitors and mobile citizens.
            policy.RequireAssertion(_ => false);
        });
});

var app = builder.Build();

// Handle unhandled exceptions centrally.
// The API will return HTTP 500 instead of exposing technical details.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
