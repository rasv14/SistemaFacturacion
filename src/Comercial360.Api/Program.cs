using Comercial360.Api.Infrastructure;
using Comercial360.Application.Catalog;
using Comercial360.Application.Sales;
using Comercial360.Adapters.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new TaxConfiguration(builder.Configuration.GetValue<decimal?>("Tax:RatePercent")));
builder.Services.AddScoped<ISaleRegistrationService, SaleRegistrationService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;
