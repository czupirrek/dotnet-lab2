using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;




namespace dotnet_lab2_cli
{
    public class DbTrack
    {
        [Key]
        public int Id { get; set; }
        public string ArtistName { get; set; }
        public string TrackName { get; set; }
        public string Album { get; set; }
        public string? Date { get; set; }
        public long Timestamp { get; set; }

        public string AlbumMbid { get; set;}

        public int ArtistId { get; set; }
        public DbArtist Artist { get; set; }

    }

    public class DbAlbum
    {
        [Key]
        public int Id { get; set; }
        public string AlbumName { get; set; }
        public string Artist { get; set; }
        public string AlbumMbid { get; set; }
        public string ImageUrl { get; set; }

    }

    public class DbArtist
    {
        [Key]
        public int Id { get; set; }
        public string ArtistName { get; set; }
        //public string ArtistMbid { get; set; }
        public string ImageUrl { get; set; }
        public List<DbTrack> Tracks { get; set; }
    }


    public class LastfmContext : DbContext
    {
        public DbSet<DbTrack> Tracks { get; set; }
        public DbSet<DbAlbum> Albums { get; set; }
        public DbSet<DbArtist> Artists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("Server=10.154.37.4;Database=lastfm;User=root;Password=aleksanderthegreat;",
                new MySqlServerVersion(new Version(8, 0, 21)));
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbTrack>()
                .HasOne(t => t.Artist)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.ArtistId);
        }
    }

}
