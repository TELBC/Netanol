﻿// <auto-generated />
using System;
using System.Net;
using Fennec.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fennec.Migrations
{
    [DbContext(typeof(PackratContext))]
    [Migration("20230923140358_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Fennec")
                .HasAnnotation("ProductVersion", "6.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fennec.Database.Domain.Layout.CompressedGroup", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("GraphNodeId")
                        .HasColumnType("bigint");

                    b.Property<long>("NetworkDeviceId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("GraphNodeId");

                    b.HasIndex("NetworkDeviceId");

                    b.ToTable("CompressedGroups", "Fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.GraphNode", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("LayoutPresetId")
                        .HasColumnType("bigint");

                    b.Property<long>("LayoutResetId")
                        .HasColumnType("bigint");

                    b.Property<long?>("NetworkDeviceId1")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("LayoutPresetId");

                    b.HasIndex("NetworkDeviceId1");

                    b.ToTable("GraphNodes", "Fennec");

                    b.HasDiscriminator<string>("Discriminator").HasValue("GraphNode");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.IslandGroup", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.HasKey("Id");

                    b.ToTable("IslandGroups", "Fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.LayoutPreset", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("LayoutPresets", "Fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.NetworkDevice", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("DnsName")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.HasKey("Id");

                    b.ToTable("NetworkDevices", "Fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.NetworkHost", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<IPAddress>("IpAddress")
                        .IsRequired()
                        .HasColumnType("inet");

                    b.Property<long?>("NetworkDeviceId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("NetworkDeviceId");

                    b.ToTable("NetworkHosts", "Fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.SingleTrace", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("ByteCount")
                        .HasColumnType("integer");

                    b.Property<long>("DestinationHostId")
                        .HasColumnType("bigint");

                    b.Property<int>("DestinationPort")
                        .HasColumnType("integer");

                    b.Property<IPAddress>("ExporterIp")
                        .IsRequired()
                        .HasColumnType("inet");

                    b.Property<int>("PacketCount")
                        .HasColumnType("integer");

                    b.Property<int>("Protocol")
                        .HasColumnType("integer");

                    b.Property<long>("SourceHostId")
                        .HasColumnType("bigint");

                    b.Property<int>("SourcePort")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DestinationHostId");

                    b.HasIndex("SourceHostId");

                    b.ToTable("SingleTraces", "Fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.DeviceNode", b =>
                {
                    b.HasBaseType("Fennec.Database.Domain.Layout.GraphNode");

                    b.Property<long?>("IslandGroupId")
                        .HasColumnType("bigint");

                    b.Property<long>("NetworkDeviceId")
                        .HasColumnType("bigint");

                    b.HasIndex("IslandGroupId");

                    b.HasIndex("NetworkDeviceId");

                    b.HasDiscriminator().HasValue("DeviceNode");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.CompressedGroup", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Layout.GraphNode", "GraphNode")
                        .WithMany()
                        .HasForeignKey("GraphNodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fennec.Database.Domain.Technical.NetworkDevice", "NetworkDevice")
                        .WithMany()
                        .HasForeignKey("NetworkDeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GraphNode");

                    b.Navigation("NetworkDevice");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.GraphNode", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Layout.LayoutPreset", "LayoutPreset")
                        .WithMany("GraphNodes")
                        .HasForeignKey("LayoutPresetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fennec.Database.Domain.Technical.NetworkDevice", null)
                        .WithMany("GraphNodes")
                        .HasForeignKey("NetworkDeviceId1");

                    b.Navigation("LayoutPreset");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.NetworkHost", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Technical.NetworkDevice", null)
                        .WithMany("NetworkHosts")
                        .HasForeignKey("NetworkDeviceId");

                    b.OwnsOne("Fennec.Database.Domain.Technical.DnsInformation", "DnsInformation", b1 =>
                        {
                            b1.Property<long>("NetworkHostId")
                                .HasColumnType("bigint");

                            b1.Property<string>("DnsName")
                                .IsRequired()
                                .HasMaxLength(250)
                                .HasColumnType("character varying(250)");

                            b1.Property<DateTimeOffset>("LastAccessedDnsName")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<long>("NetworkDeviceId")
                                .HasColumnType("bigint");

                            b1.HasKey("NetworkHostId");

                            b1.HasIndex("NetworkDeviceId");

                            b1.ToTable("NetworkHosts", "Fennec");

                            b1.HasOne("Fennec.Database.Domain.Technical.NetworkDevice", "NetworkDevice")
                                .WithMany()
                                .HasForeignKey("NetworkDeviceId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner()
                                .HasForeignKey("NetworkHostId");

                            b1.Navigation("NetworkDevice");
                        });

                    b.Navigation("DnsInformation");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.SingleTrace", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Technical.NetworkHost", "DestinationHost")
                        .WithMany()
                        .HasForeignKey("DestinationHostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fennec.Database.Domain.Technical.NetworkHost", "SourceHost")
                        .WithMany()
                        .HasForeignKey("SourceHostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DestinationHost");

                    b.Navigation("SourceHost");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.DeviceNode", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Layout.IslandGroup", "IslandGroup")
                        .WithMany("Members")
                        .HasForeignKey("IslandGroupId");

                    b.HasOne("Fennec.Database.Domain.Technical.NetworkDevice", "NetworkDevice")
                        .WithMany()
                        .HasForeignKey("NetworkDeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("IslandGroup");

                    b.Navigation("NetworkDevice");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.IslandGroup", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.LayoutPreset", b =>
                {
                    b.Navigation("GraphNodes");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.NetworkDevice", b =>
                {
                    b.Navigation("GraphNodes");

                    b.Navigation("NetworkHosts");
                });
#pragma warning restore 612, 618
        }
    }
}