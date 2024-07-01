using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JuiceShopDotNet.Unsafe.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        //public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<CreditApplication> CreditApplications { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductReview> ProductReviews { get; set; }
        public virtual DbSet<ProductReview_Display> ProductReview_Displays { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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
                entity.Property(e => e.SocialSecurityNumber).HasMaxLength(11);
                entity.Property(e => e.EmploymentStatus).HasMaxLength(15);
                entity.Property(e => e.SubmittedOn).HasColumnType("datetime");
                entity.Property(e => e.Approver).HasMaxLength(450);
                entity.Property(e => e.DecisionDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey("OrderID");
                entity.Property(e => e.UserID).HasMaxLength(450);
                entity.Property(e => e.BillingPostalCode).HasMaxLength(25);
                entity.Property(e => e.CreditCardNumber).HasMaxLength(16);
                entity.Property(e => e.CardExpirationMonth).HasMaxLength(2);
                entity.Property(e => e.CardExpirationYear).HasMaxLength(2);
                entity.Property(e => e.CardCvcNumber).HasMaxLength(3);
                entity.Property(e => e.PaymentID).HasMaxLength(200);

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
                entity.Property(e => e.UserID).HasMaxLength(450);
            });

            modelBuilder.Entity<ProductReview_Display>(entity =>
            {
                entity.ToView("ProductReview_Display").HasNoKey();
            });

            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<AspNetUser>(entity =>
            //{
            //    entity.Property(e => e.Email).HasMaxLength(256);
            //    entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            //    entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            //    entity.Property(e => e.UserName).HasMaxLength(256);

            //    entity.HasOne<IdentityUser>().WithOne()
            //        .HasForeignKey<IdentityUser>(e => e.Id);
            //    //entity.HasMany(d => d.Roles).WithMany(p => p.Users)
            //    //    .UsingEntity<Dictionary<string, object>>(
            //    //        "AspNetUserRole",
            //    //        r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
            //    //        l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
            //    //        j =>
            //    //        {
            //    //            j.HasKey("UserId", "RoleId");
            //    //            j.ToTable("AspNetUserRoles");
            //    //        });
            //});
        }
    }
}
