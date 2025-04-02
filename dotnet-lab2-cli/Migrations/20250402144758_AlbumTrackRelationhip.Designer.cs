﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using dotnet_lab2_cli;

#nullable disable

namespace dotnet_lab2_cli.Migrations
{
    [DbContext(typeof(LastfmContext))]
    [Migration("20250402144758_AlbumTrackRelationhip")]
    partial class AlbumTrackRelationhip
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("dotnet_lab2_cli.DbAlbum", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlbumMbid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("AlbumName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("dotnet_lab2_cli.DbArtist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ArtistName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("dotnet_lab2_cli.DbTrack", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Album")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("AlbumId")
                        .HasColumnType("int");

                    b.Property<string>("AlbumMbid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("ArtistId")
                        .HasColumnType("int");

                    b.Property<string>("ArtistName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Date")
                        .HasColumnType("longtext");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("TrackName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("AlbumId");

                    b.HasIndex("ArtistId");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("dotnet_lab2_cli.DbTrack", b =>
                {
                    b.HasOne("dotnet_lab2_cli.DbAlbum", "DbAlbum")
                        .WithMany("Tracks")
                        .HasForeignKey("AlbumId");

                    b.HasOne("dotnet_lab2_cli.DbArtist", "Artist")
                        .WithMany("Tracks")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");

                    b.Navigation("DbAlbum");
                });

            modelBuilder.Entity("dotnet_lab2_cli.DbAlbum", b =>
                {
                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("dotnet_lab2_cli.DbArtist", b =>
                {
                    b.Navigation("Tracks");
                });
#pragma warning restore 612, 618
        }
    }
}
