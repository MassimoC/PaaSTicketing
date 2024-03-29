﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaaS.Ticketing.ApiLib.Context;

namespace PaaS.Ticketing.ApiLib.Migrations
{
    [DbContext(typeof(TicketingContext))]
    partial class ConcertsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PaaS.Ticketing.Api.Entities.Concert", b =>
                {
                    b.Property<Guid>("ConcertId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("From");

                    b.Property<string>("Location")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<int>("Price");

                    b.Property<DateTime>("To");

                    b.HasKey("ConcertId");

                    b.ToTable("Concerts");
                });

            modelBuilder.Entity("PaaS.Ticketing.Api.Entities.ConcertUser", b =>
                {
                    b.Property<Guid>("ConcertId");

                    b.Property<Guid>("UserId");

                    b.Property<Guid>("ConcertUserId");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime>("DateRegistration")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Status");

                    b.Property<string>("Token");

                    b.HasKey("ConcertId", "UserId");

                    b.HasAlternateKey("ConcertUserId");

                    b.HasIndex("UserId");

                    b.ToTable("ConcertUsers");
                });

            modelBuilder.Entity("PaaS.Ticketing.Api.Entities.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("BirthDate");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("Phone");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PaaS.Ticketing.Api.Entities.ConcertUser", b =>
                {
                    b.HasOne("PaaS.Ticketing.Api.Entities.Concert", "Concert")
                        .WithMany("ConcertUser")
                        .HasForeignKey("ConcertId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaaS.Ticketing.Api.Entities.User", "User")
                        .WithMany("ConcertUser")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
