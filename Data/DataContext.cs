using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Data
{
    public class DataContext : DbContext    //An instance of the DbContext represents a session with the database.This means we can use this instance to query the database and save all the changes to our RPG characters.
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Character> Characters { get; set; }
        //Whenever you want to see a representation of your model in the database, you have to add a DbSet of this model. 
        //That’s how Entity Framework knows what tables it should create.
        public DbSet<User> Users { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<CharacterSkill> CharacterSkills { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacterSkill>()
                .HasKey(cs => new { cs.CharacterId, cs.SkillId });
        }
    }
}






//Before we can start with the actual migration, we have to provide a connection string and add the DbContext to the service collection of our application.