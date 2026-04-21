using canvasBid.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace canvasBid.Data
{
    public class dbContext: IdentityDbContext<User, IdentityRole, string>
    {
        public dbContext(DbContextOptions<dbContext> options)
        : base(options)
        {
        }

        public DbSet<Bids> Bid { get; set; }
        public DbSet<Viewbids> UserBids { get; set; }
        public DbSet<Artworks> Artwork { get; set; }
        public DbSet<ArtworkTags> ArtworkTags { get; set; }
        public DbSet<Watchlist> Watchlists { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Viewbids>()
                .HasKey(ub => new { ub.UserId, ub.BidId });

            builder.Entity<Viewbids>()
                .HasOne(x => x.User)
                .WithMany(x => x.bidsView)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Viewbids>()
                .HasOne(x => x.Bid)
                .WithMany(x => x.viewUsers)
                .HasForeignKey(x => x.BidId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Viewartworks>()
        .HasKey(x => new { x.userId, x.artworkId });

            builder.Entity<Viewartworks>()
                .HasOne(x => x.user)
                .WithMany(x => x.artworkView)
                .HasForeignKey(x => x.userId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Viewartworks>()
                .HasOne(x => x.artwork)
                .WithMany(x => x.userView)
                .HasForeignKey(x => x.artworkId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ArtworkTags>()
        .HasKey(x => new { x.ArtworkId, x.TagId });

            builder.Entity<ArtworkTags>()
                .HasOne(x => x.Artwork)
                .WithMany(x => x.ArtworkTags)
                .HasForeignKey(x => x.ArtworkId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ArtworkTags>()
                .HasOne(x => x.Tag)
                .WithMany(x => x.ArtworkTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Bids>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Bids>()
               .HasOne(b => b.Artwork)
               .WithMany(a => a.Bids)
               .HasForeignKey(b => b.ArtworkId)
               .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<Watchlist>()
    .HasKey(x => new { x.UserId, x.ArtworkId });

            builder.Entity<Watchlist>()
                .HasOne(x => x.User)
                .WithMany(x => x.Watchlists)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Watchlist>()
                .HasOne(x => x.Artwork)
                .WithMany(x => x.Watchlists)
                .HasForeignKey(x => x.ArtworkId)
                .OnDelete(DeleteBehavior.NoAction);
        }

      


    }
}
