using System;
using System.Collections.Generic;
using JuiceShopDotNet.API.Cryptography;
using JuiceShopDotNet.API.Data.Converters;
using JuiceShopDotNet.Common.Cryptography.SymmetricEncryption;
using Microsoft.EntityFrameworkCore;

namespace JuiceShopDotNet.API.Data;

public partial class DatabaseContext : DbContext
{
    private readonly IEncryptionService _encryptionService;

    public DatabaseContext(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IEncryptionService encryptionService)
        : base(options)
    {
        _encryptionService = encryptionService;
    }

    public virtual DbSet<CreditApplication> CreditApplications { get; set; }

    public virtual DbSet<JuiceShopUser> JuiceShopUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditApplication>(entity =>
        {
            entity.ToTable("CreditApplication");

            entity.Property(e => e.CreditApplicationID).ValueGeneratedNever();
            entity.Property(e => e.SocialSecurityNumber)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasConversion(new EncryptionConverter(KeyNames.CreditApplication_SocialSecurityNumber, _encryptionService));
        });

        modelBuilder.Entity<JuiceShopUser>(entity =>
        {
            entity.ToTable("JuiceShopUser");

            entity.Property(e => e.JuiceShopUserID).ValueGeneratedNever();
            entity.Property(e => e.UserEmail)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasConversion(new EncryptionConverter(KeyNames.JuiceShopUser_UserEmail, _encryptionService));
            entity.Property(e => e.UserName)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasConversion(new EncryptionConverter(KeyNames.JuiceShopUser_Username, _encryptionService));
            entity.Property(e => e.NormalizedUserEmail)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasConversion(new EncryptionConverter(KeyNames.JuiceShopUser_NormalizedUserEmail, _encryptionService));
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
