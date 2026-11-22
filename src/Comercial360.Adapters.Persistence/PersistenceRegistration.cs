using Comercial360.Application.Sales;
using Comercial360.Application.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comercial360.Adapters.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Commercial360");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Falta ConnectionStrings:Commercial360.");
        }

        services.AddDbContext<Comercial360DbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ISaleCatalog, EfSaleCatalog>();
        services.AddScoped<ISaleRegistrationStore, EfSaleRegistrationStore>();
        services.AddScoped<IElectronicDocumentQuery, EfElectronicDocumentQuery>();
        services.AddScoped<ICatalogStore, EfCatalogStore>();
        services.AddScoped<ISaleQuery, EfSaleQuery>();
        return services;
    }
}
