var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Centralized API error responses
builder.Services.AddProblemDetails();

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

app.UseAuthorization();

app.MapControllers();

app.Run();