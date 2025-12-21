using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Dom;

public partial class CarShowroomDbContext : DbContext
{
    public CarShowroomDbContext(DbContextOptions<CarShowroomDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Addition> Additions { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarType> CarTypes { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ConditionType> ConditionTypes { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<EngineType> EngineTypes { get; set; }

    public virtual DbSet<Model> Models { get; set; }


    public virtual DbSet<RoleType> RoleTypes { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleAddition> SaleAdditions { get; set; }

    public virtual DbSet<SaleDiscount> SaleDiscounts { get; set; }

    public virtual DbSet<Transmission> Transmissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wdtype> Wdtypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Addition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Additions_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Brand_pkey");

            entity.ToTable("Brand");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CountryId).HasColumnName("Country_Id");

            entity.HasOne(d => d.Country).WithMany(p => p.Brands)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("Brand_Country_Id_fkey");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Car_pkey");

            entity.ToTable("Car");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ConditionId).HasColumnName("Condition_Id");
            entity.Property(e => e.EngTypeId).HasColumnName("EngType_Id");
            entity.Property(e => e.EngVol).HasColumnName("Eng_Vol");
            entity.Property(e => e.TransmissionId).HasColumnName("Transmission_Id");
            entity.Property(e => e.TypeId).HasColumnName("Type_Id");
            entity.Property(e => e.WdId).HasColumnName("WD_Id");

            entity.HasOne(d => d.Condition).WithMany(p => p.Cars)
                .HasForeignKey(d => d.ConditionId)
                .HasConstraintName("Car_Condition_Id_fkey");

            entity.HasOne(d => d.EngType).WithMany(p => p.Cars)
                .HasForeignKey(d => d.EngTypeId)
                .HasConstraintName("Car_EngType_Id_fkey");

            entity.HasOne(d => d.Model).WithMany(p => p.Cars)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("Car_ModelId_fkey");

            entity.HasOne(d => d.Transmission).WithMany(p => p.Cars)
                .HasForeignKey(d => d.TransmissionId)
                .HasConstraintName("Car_Transmission_Id_fkey");

            entity.HasOne(d => d.Type).WithMany(p => p.Cars)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("Car_Type_Id_fkey");

            entity.HasOne(d => d.Wd).WithMany(p => p.Cars)
                .HasForeignKey(d => d.WdId)
                .HasConstraintName("Car_WD_Id_fkey");
        });

        modelBuilder.Entity<CarType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CarType_pkey");

            entity.ToTable("CarType");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Client_pkey");

            entity.ToTable("Client");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.PassData).HasColumnName("Pass_Data");
            entity.Property(e => e.PhoneNumber).HasColumnName("Phone_Number");
        });

        modelBuilder.Entity<ConditionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ConditionType_pkey");

            entity.ToTable("ConditionType");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Country_pkey");

            entity.ToTable("Country");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Discount_pkey");

            entity.ToTable("Discount");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<EngineType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EngineType_pkey");

            entity.ToTable("EngineType");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Model_pkey");

            entity.ToTable("Model");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BrandId).HasColumnName("Brand_Id");

            entity.HasOne(d => d.Brand).WithMany(p => p.Models)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("Model_Brand_Id_fkey");
        });

        modelBuilder.Entity<RoleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RoleType_pkey");

            entity.ToTable("RoleType");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Sale_pkey");

            entity.ToTable("Sale");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CarId).HasColumnName("Car_Id");
            entity.Property(e => e.ClientId).HasColumnName("Client_Id");
            entity.Property(e => e.ManagerId).HasColumnName("Manager_Id");

            entity.HasOne(d => d.Car).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Sale_Car_Id_fkey");

            entity.HasOne(d => d.Client).WithMany(p => p.Sales)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("Sale_Client_Id_fkey");

            entity.HasOne(d => d.Manager).WithMany(p => p.Sales)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("Sale_Manager_Id_fkey");
        });

        modelBuilder.Entity<SaleAddition>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Sale_Additions");

            entity.Property(e => e.AddId).HasColumnName("Add_Id");
            entity.Property(e => e.SaleId).HasColumnName("Sale_Id");

            entity.HasOne(d => d.Add).WithMany()
                .HasForeignKey(d => d.AddId)
                .HasConstraintName("Sale_Additions_Add_Id_fkey");

            entity.HasOne(d => d.Sale).WithMany()
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("Sale_Additions_Sale_Id_fkey");
        });

        modelBuilder.Entity<SaleDiscount>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Sale_Discount");

            entity.Property(e => e.DiscountId).HasColumnName("Discount_Id");
            entity.Property(e => e.SaleId).HasColumnName("Sale_Id");

            entity.HasOne(d => d.Discount).WithMany()
                .HasForeignKey(d => d.DiscountId)
                .HasConstraintName("Sale_Discount_Discount_Id_fkey");

            entity.HasOne(d => d.Sale).WithMany()
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("Sale_Discount_Sale_Id_fkey");
        });

        modelBuilder.Entity<Transmission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Transmission_pkey");

            entity.ToTable("Transmission");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.ToTable("User");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RoleTypeId).HasColumnName("RoleType_Id");

            entity.HasOne(d => d.RoleType).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleTypeId)
                .HasConstraintName("User_RoleType_Id_fkey");
        });

        modelBuilder.Entity<Wdtype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("WDType_pkey");

            entity.ToTable("WDType");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
