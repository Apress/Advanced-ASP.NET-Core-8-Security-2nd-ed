using JuiceShopDotNet.Common.Cryptography.Hashing;
using JuiceShopDotNet.Safe.Data.HardCodedFilters;
using JuiceShopDotNet.Safe.Data.ValueConverters;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IHashingService _hashingService;

    public virtual DbSet<CreditApplication> CreditApplications { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderProduct> OrderProducts { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHashingService hashingService)
        : base(options)
    {
        _hashingService = hashingService;

        this.SavingChanges += ApplicationDbContext_SavingChanges;
    }

    private void ApplicationDbContext_SavingChanges(object? sender, SavingChangesEventArgs e)
    {
        int i = 1;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditApplication>(entity =>
        {
            entity.ToTable("CreditApplication");

            entity.HasKey("CreditApplicationID");
            entity.Property(e => e.UserID).HasMaxLength(450);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Birthdate).HasColumnType("datetime");
            entity.Property(e => e.SocialSecurityNumber).HasMaxLength(100);
            entity.Property(e => e.EmploymentStatus).HasMaxLength(15);
            entity.Property(e => e.SubmittedOn).HasColumnType("datetime");
            entity.Property(e => e.Approver).HasMaxLength(450);
            entity.Property(e => e.DecisionDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");

            entity.HasKey("OrderID");
            entity.Property(e => e.BillingPostalCode).HasMaxLength(25);
            entity.Property(e => e.CreditCardLastFour).HasMaxLength(4);
            entity.Property(e => e.PaymentID).HasMaxLength(200);
            entity.Property(e => e.OrderCompletedOn).HasColumnType("datetime");

            entity.HasMany(e => e.OrderProducts).WithOne(e => e.Order).HasForeignKey(e => e.OrderID);
        });

        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.ToTable("OrderProduct");

            entity.HasKey("OrderProductID");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey("id");

            entity.HasMany(e => e.ProductReviews).WithOne(e => e.Product).HasForeignKey(e => e.ProductID);
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.ToTable("ProductReviews");

            entity.HasKey("ProductReviewID");
            entity.Property(e => e.ReviewText).HasConversion(new IntegrityHashConverter("ProductReview_ReviewText_Salt", _hashingService));
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

            entity.HasOne(e => e.Product).WithMany(e => e.ProductReviews).HasForeignKey(e => e.ProductID);
        });

        base.OnModelCreating(modelBuilder);
    }

    public UserFilter FilterByUser(ClaimsPrincipal user)
    {
        return new UserFilter(this, user);
    }
}
