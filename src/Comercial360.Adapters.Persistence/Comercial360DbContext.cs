using Comercial360.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comercial360.Adapters.Persistence;

public sealed class Comercial360DbContext(DbContextOptions<Comercial360DbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ElectronicDocument> ElectronicDocuments => Set<ElectronicDocument>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.DocumentType).HasMaxLength(20).IsRequired();
            entity.Property(customer => customer.DocumentNumber).HasMaxLength(20).IsRequired();
            entity.Property(customer => customer.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(customer => new { customer.DocumentType, customer.DocumentNumber }).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Code).HasMaxLength(50).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(300).IsRequired();
            entity.Property(product => product.UnitNetPrice).HasPrecision(18, 4);
            entity.Property(product => product.Stock).HasPrecision(18, 4);
            entity.HasIndex(product => product.Code).IsUnique();
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sales");
            entity.HasKey(sale => sale.Id);
            entity.Property(sale => sale.RequestedBy).HasMaxLength(200).IsRequired();
            entity.Property(sale => sale.Status).HasMaxLength(40).IsRequired();
            entity.Property(sale => sale.Subtotal).HasPrecision(18, 2);
            entity.Property(sale => sale.TaxTotal).HasPrecision(18, 2);
            entity.Property(sale => sale.Total).HasPrecision(18, 2);
            entity.HasOne<Customer>().WithMany().HasForeignKey(sale => sale.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(sale => sale.Lines).WithOne(line => line.Sale).HasForeignKey(line => line.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(sale => sale.CreatedAtUtc);
        });

        modelBuilder.Entity<SaleLine>(entity =>
        {
            entity.ToTable("SaleLines");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.DescriptionSnapshot).HasMaxLength(300).IsRequired();
            entity.Property(line => line.Quantity).HasPrecision(18, 4);
            entity.Property(line => line.UnitNetPrice).HasPrecision(18, 4);
            entity.Property(line => line.NetAmount).HasPrecision(18, 2);
            entity.Property(line => line.TaxAmount).HasPrecision(18, 2);
            entity.HasOne<Product>().WithMany().HasForeignKey(line => line.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");
            entity.HasKey(movement => movement.Id);
            entity.Property(movement => movement.Type).HasMaxLength(40).IsRequired();
            entity.Property(movement => movement.Quantity).HasPrecision(18, 4);
            entity.HasOne<Product>().WithMany().HasForeignKey(movement => movement.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Sale>().WithMany().HasForeignKey(movement => movement.SaleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(movement => new { movement.SaleId, movement.ProductId });
        });

        modelBuilder.Entity<ElectronicDocument>(entity =>
        {
            entity.ToTable("ElectronicDocuments");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.DocumentType).HasMaxLength(30).IsRequired();
            entity.Property(document => document.Status).HasConversion<string>().HasMaxLength(40);
            entity.Property(document => document.Series).HasMaxLength(20);
            entity.Property(document => document.Number).HasMaxLength(30);
            entity.Property(document => document.ExternalReference).HasMaxLength(300);
            entity.Property(document => document.NormalizedMessage).HasMaxLength(2000);
            entity.HasOne(document => document.Sale).WithOne().HasForeignKey<ElectronicDocument>(document => document.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(document => document.SaleId).IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.EventType).HasMaxLength(100).IsRequired();
            entity.Property(message => message.Payload).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(message => message.Status).HasMaxLength(30).IsRequired();
            entity.HasOne<ElectronicDocument>().WithMany().HasForeignKey(message => message.ElectronicDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(message => new { message.Status, message.NextAttemptAtUtc });
            entity.HasIndex(message => message.ElectronicDocumentId).IsUnique();
        });

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.ToTable("IdempotencyRecords");
            entity.HasKey(record => record.IdempotencyKey);
            entity.Property(record => record.IdempotencyKey).HasMaxLength(128);
            entity.Property(record => record.RequestHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(record => record.SaleId).IsUnique();
        });
    }
}
