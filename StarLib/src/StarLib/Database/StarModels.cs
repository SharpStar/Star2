using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Framework.Configuration;

namespace StarLib.Database
{
    public class StarDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<CharacterIp> CharacterIps { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<EventHistory> EventHistory { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=star.db;");
        }
    }

    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public bool Admin { get; set; }

        public bool Banned { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public DateTime? LastLogin { get; set; }

        public int? GroupId { get; set; }

        public Group Group { get; set; }

        public List<Character> Characters { get; set; }

        public List<EventHistory> History { get; set; }
    }

    public class Character
    {
        public int CharacterId { get; set; }

        public string Name { get; set; }

        public string Uuid { get; set; }

        public string LastIpAddress { get; set; }

        public int? UserId { get; set; }

        public User User { get; set; }

        public List<CharacterIp> CharacterIps { get; set; }
    }

    public class CharacterIp
    {
        public int CharacterIpId { get; set; }

        public string Address { get; set; }

        public int CharacterId { get; set; }

        public Character Character { get; set; }
    }

    public class Group
    {
        public int GroupId { get; set; }

        public string Name { get; set; }

        public bool Default { get; set; }

        public List<User> Users { get; set; }
    }

    public class Ban
    {
        public int BanId { get; set; }
        
        public string Reason { get; set; }

        public bool Active { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public int CharacterId { get; set; }

        public Character Character { get; set; }

        public int? UserId { get; set; }

        public User User { get; set; }
    }

    public class EventHistory
    {
        public int EventHistoryId { get; set; }

        public string EventType { get; set; }

        public string Contents { get; set; }

        public int? UserId { get; set; }

        public User User { get; set; }
    }
}
