using CherkasovLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace CherkasovLibrary
{
    /// <summary>
    /// Контекст базы данных для EntityFramework Core
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<PartnerType> PartnerTypes { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Строка подключения к PostgreSQL
            optionsBuilder.UseNpgsql("Host=localhost;Database=cherkasov;Username=app;Password=123456789");

            // Для отладки (можно убрать в релизе)
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Указываем точные имена таблиц
            modelBuilder.Entity<PartnerType>().ToTable("partner_types_cherkasov");
            modelBuilder.Entity<Partner>().ToTable("partners_cherkasov");
            modelBuilder.Entity<Product>().ToTable("products_cherkasov");
            modelBuilder.Entity<Sale>().ToTable("sales_cherkasov");

            // Настройка PartnerType
            modelBuilder.Entity<PartnerType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            });

            // Настройка Partner
            modelBuilder.Entity<Partner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
                entity.Property(e => e.DirectorFullname).HasColumnName("director_fullname").HasMaxLength(200);
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                // Связь с PartnerType
                entity.HasOne(e => e.PartnerType)
                    .WithMany()
                    .HasForeignKey(e => e.TypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь с Sales
                entity.HasMany(e => e.Sales)
                    .WithOne(e => e.Partner)
                    .HasForeignKey(e => e.PartnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Article).HasColumnName("article").HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            });

            // Настройка Sale
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PartnerId).HasColumnName("partner_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.SaleDate).HasColumnName("sale_date");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                // Связи
                entity.HasOne(e => e.Partner)
                    .WithMany(e => e.Sales)
                    .HasForeignKey(e => e.PartnerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конвертация DateTime для PostgreSQL
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Sale>()
                .Property(e => e.SaleDate)
                .HasConversion(dateTimeConverter);

            modelBuilder.Entity<Partner>()
                .Property(e => e.CreatedAt)
                .HasConversion(dateTimeConverter);

            modelBuilder.Entity<Partner>()
                .Property(e => e.UpdatedAt)
                .HasConversion(dateTimeConverter);
        }
    }
}