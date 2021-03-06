﻿// <auto-generated />
using CloudNine.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CloudNine.Core.Migrations
{
    [DbContext(typeof(CloudNineDatabaseModel))]
    [Migration("20201231185514_ModNoticesFavQuotes")]
    partial class ModNoticesFavQuotes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("CloudNine.Core.Configuration.DiscordGuildConfiguration", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BirthdayConfiguration")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FavoriteQuotes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HiddenQuotes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Prefix")
                        .HasColumnType("TEXT");

                    b.Property<string>("Quotes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ServerConfigurations");
                });

            modelBuilder.Entity("CloudNine.Core.Moderation.ModCore", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModlogNotices")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("WarnSet")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("GuildId");

                    b.ToTable("Moderation");
                });
#pragma warning restore 612, 618
        }
    }
}
