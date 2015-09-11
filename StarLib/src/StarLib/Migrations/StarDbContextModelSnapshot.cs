using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using StarLib.Database;

namespace StarLib.Migrations
{
    [DbContext(typeof(StarDbContext))]
    partial class StarDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Annotation("ProductVersion", "7.0.0-beta8-15657");

            modelBuilder.Entity("StarLib.Database.Ban", b =>
                {
                    b.Property<int>("BanId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<int>("CharacterId");

                    b.Property<DateTime?>("ExpirationTime");

                    b.Property<string>("Reason");

                    b.Property<int?>("UserId");

                    b.Key("BanId");
                });

            modelBuilder.Entity("StarLib.Database.Character", b =>
                {
                    b.Property<int>("CharacterId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("LastIpAddress");

                    b.Property<string>("Name");

                    b.Property<int?>("UserId");

                    b.Property<string>("Uuid");

                    b.Key("CharacterId");
                });

            modelBuilder.Entity("StarLib.Database.CharacterIp", b =>
                {
                    b.Property<int>("CharacterIpId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<int>("CharacterId");

                    b.Key("CharacterIpId");
                });

            modelBuilder.Entity("StarLib.Database.EventHistory", b =>
                {
                    b.Property<int>("EventHistoryId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Contents");

                    b.Property<string>("EventType");

                    b.Property<int?>("UserId");

                    b.Key("EventHistoryId");
                });

            modelBuilder.Entity("StarLib.Database.Group", b =>
                {
                    b.Property<int>("GroupId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Default");

                    b.Property<string>("Name");

                    b.Key("GroupId");
                });

            modelBuilder.Entity("StarLib.Database.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Admin");

                    b.Property<bool>("Banned");

                    b.Property<int?>("GroupId");

                    b.Property<DateTime?>("LastLogin");

                    b.Property<byte[]>("PasswordHash");

                    b.Property<byte[]>("PasswordSalt");

                    b.Property<string>("Username");

                    b.Key("UserId");
                });

            modelBuilder.Entity("StarLib.Database.Ban", b =>
                {
                    b.Reference("StarLib.Database.Character")
                        .InverseCollection()
                        .ForeignKey("CharacterId");

                    b.Reference("StarLib.Database.User")
                        .InverseCollection()
                        .ForeignKey("UserId");
                });

            modelBuilder.Entity("StarLib.Database.Character", b =>
                {
                    b.Reference("StarLib.Database.User")
                        .InverseCollection()
                        .ForeignKey("UserId");
                });

            modelBuilder.Entity("StarLib.Database.CharacterIp", b =>
                {
                    b.Reference("StarLib.Database.Character")
                        .InverseCollection()
                        .ForeignKey("CharacterId");
                });

            modelBuilder.Entity("StarLib.Database.EventHistory", b =>
                {
                    b.Reference("StarLib.Database.User")
                        .InverseCollection()
                        .ForeignKey("UserId");
                });

            modelBuilder.Entity("StarLib.Database.User", b =>
                {
                    b.Reference("StarLib.Database.Group")
                        .InverseCollection()
                        .ForeignKey("GroupId");
                });
        }
    }
}
