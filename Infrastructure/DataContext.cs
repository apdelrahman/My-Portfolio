using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // الضيف الجديد
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure
{
    // 1. تغيير الوراثة لتشمل ApplicationUser
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 2. مهم جداً استدعاء الـ base أولاً عشان جداول Identity تترسم
            base.OnModelCreating(modelBuilder);

            // إعداداتك الأصلية زي ما هي بالظبط
            modelBuilder.Entity<Owner>().ToTable("Owners");
            modelBuilder.Entity<PortofolioItem>().ToTable("PortfolioItem");
            modelBuilder.Entity<PortfolioImage>().ToTable("PortfolioImages");
            modelBuilder.Entity<Contact>().ToTable("Contacts");

            modelBuilder.Entity<Owner>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<PortofolioItem>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<PortfolioImage>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Contact>().Property(x => x.Id).HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<PortfolioImage>()
                .HasOne(pi => pi.PortofolioItem)
                .WithMany(p => p.ProjectImages)
                .HasForeignKey(pi => pi.PortofolioItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Owner> Owner { get; set; }
        public DbSet<PortofolioItem> PortfolioItem { get; set; }
        public DbSet<PortfolioImage> PortfolioImages { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }
}