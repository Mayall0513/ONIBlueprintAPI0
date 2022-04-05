using Microsoft.EntityFrameworkCore;

using BlueprintRepository.Models;

namespace BlueprintRepository.Repositories {
    public sealed class BlueprintsDbContext : DbContext {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<UserRoleModel> UserRoles { get; set; }
        public DbSet<UserTokenModel> UserTokens { get; set; }

        public BlueprintsDbContext(DbContextOptions<BlueprintsDbContext> options) : base(options) {
            Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS blueprints_database.Users (
                    ID VARCHAR(64) NOT NULL,
                    Username VARCHAR(16) NOT NULL,
                    Email VARCHAR(320) NOT NULL,
                    PasswordHash VARCHAR(84) NOT NULL,
                    LastLogin DATETIME NULL,
                    EmailVerificationToken VARCHAR(50) NULL,
                    EmailVerificationTokenDate DATETIME NULL,
                    EmailVerificationTokenExpiration DATETIME NULL,
                    CreationDate DATETIME NOT NULL,
                    PRIMARY KEY (ID)
                )
            ");

            Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS blueprints_database.UserRoles (
                    UserID VARCHAR(64) NOT NULL,
                    RoleID VARCHAR(64) NOT NULL,
                    CreationDate DATETIME NOT NULL,
                    PRIMARY KEY (UserID, RoleID),
                    FOREIGN KEY (UserID) REFERENCES blueprints_database.Users(ID)
                )
            ");

            Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS blueprints_database.UserTokens (
                    UserID VARCHAR(64) NOT NULL,
                    TokenID VARCHAR(64) NOT NULL,
                    TokenExpirationDate DATETIME NOT NULL,
                    CreationDate DATETIME NOT NULL,
                    PRIMARY KEY (UserID, TokenID),
                    FOREIGN KEY (UserID) REFERENCES blueprints_database.Users(ID)
                )
            ");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.Entity<UserModel>().ToTable("Users");
            modelBuilder.Entity<UserModel>().HasKey("Id");

            modelBuilder.Entity<UserRoleModel>().ToTable("UserRoles");
            modelBuilder.Entity<UserRoleModel>().HasKey("UserId", "RoleId");
            modelBuilder.Entity<UserRoleModel>().HasOne(x => x.User).WithMany(x => x.UserRoleModels).HasForeignKey(x => x.UserId);

            modelBuilder.Entity<UserTokenModel>().ToTable("UserTokens");
            modelBuilder.Entity<UserTokenModel>().HasKey("UserId", "TokenId");
            modelBuilder.Entity<UserTokenModel>().HasOne(x => x.User).WithMany(x => x.UserTokenModels).HasForeignKey(x => x.UserId);
        }
    }
}
