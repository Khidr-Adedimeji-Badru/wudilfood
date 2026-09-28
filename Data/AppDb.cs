using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WudilFood.Models;
namespace WudilFood.Data;

public class AppDb(DbContextOptions<AppDb> options):IdentityDbContext<AppUser>(options)
{
    public DbSet<Vendor> Vendors=>Set<Vendor>();
    public DbSet<Category> Categories=>Set<Category>();
    public DbSet<MenuItem> MenuItems=>Set<MenuItem>();
    public DbSet<FoodSchedule> FoodSchedules=>Set<FoodSchedule>();
    public DbSet<VendorClosure> VendorClosures=>Set<VendorClosure>();
    public DbSet<Rider> Riders=>Set<Rider>();
    public DbSet<FoodOrder> Orders=>Set<FoodOrder>();
    public DbSet<OrderLine> OrderLines=>Set<OrderLine>();
    public DbSet<OrderStatusHistory> OrderStatusHistories=>Set<OrderStatusHistory>();
    public DbSet<Favorite> Favorites=>Set<Favorite>();
    public DbSet<Review> Reviews=>Set<Review>();
    public DbSet<Notice> Notices=>Set<Notice>();
    public DbSet<Complaint> Complaints=>Set<Complaint>();
    public DbSet<AuditLog> AuditLogs=>Set<AuditLog>();
    public DbSet<PlatformSetting> Settings=>Set<PlatformSetting>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Vendor>().HasIndex(x=>x.UserId).IsUnique();
        b.Entity<Category>().HasIndex(x=>x.Name).IsUnique();
        b.Entity<FoodOrder>().HasIndex(x=>x.Reference).IsUnique();
        b.Entity<Review>().HasIndex(x=>x.FoodOrderId).IsUnique();
        b.Entity<Favorite>().HasIndex(x=>new{x.CustomerId,x.MenuItemId}).IsUnique();
        b.Entity<VendorClosure>().HasIndex(x=>new{x.VendorId,x.StartsUtc,x.EndsUtc});
        b.Entity<AuditLog>().HasIndex(x=>x.CreatedUtc);
        b.Entity<MenuItem>().Property(x=>x.Stock).IsConcurrencyToken();
        b.Entity<FoodOrder>().HasOne(x=>x.Customer).WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<FoodOrder>().HasOne(x=>x.Vendor).WithMany().HasForeignKey(x=>x.VendorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<FoodOrder>().HasOne(x=>x.Rider).WithMany(x=>x.Orders).HasForeignKey(x=>x.RiderId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<OrderLine>().HasOne(x=>x.MenuItem).WithMany().HasForeignKey(x=>x.MenuItemId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<OrderStatusHistory>().HasOne(x=>x.Order).WithMany().HasForeignKey(x=>x.FoodOrderId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Favorite>().HasOne(x=>x.Customer).WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Favorite>().HasOne(x=>x.MenuItem).WithMany().HasForeignKey(x=>x.MenuItemId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Review>().HasOne(x=>x.Order).WithOne(x=>x.Review).HasForeignKey<Review>(x=>x.FoodOrderId);
        foreach(var entityType in new[]{typeof(MenuItem),typeof(FoodOrder),typeof(OrderLine),typeof(PlatformSetting)})
            foreach(var property in b.Entity(entityType).Metadata.GetProperties().Where(p=>p.ClrType==typeof(decimal)))
            {
                property.SetPrecision(14);
                property.SetScale(2);
            }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken=default)
    {
        foreach(var entry in ChangeTracker.Entries<FoodOrder>().ToList())
        {
            if(entry.State==EntityState.Added)
            {
                OrderStatusHistories.Add(new OrderStatusHistory{Order=entry.Entity,ToState=entry.Entity.State,ChangedBy="Order created"});
            }
            else if(entry.State==EntityState.Modified&&entry.Property(x=>x.State).IsModified)
            {
                var from=entry.Property(x=>x.State).OriginalValue;
                var to=entry.Property(x=>x.State).CurrentValue;
                OrderStatusHistories.Add(new OrderStatusHistory{FoodOrderId=entry.Entity.Id,FromState=from,ToState=to,ChangedBy="Order workflow"});
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

