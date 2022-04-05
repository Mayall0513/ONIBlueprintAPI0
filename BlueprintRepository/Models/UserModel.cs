using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BlueprintRepository.Models {
    public class UserModel : Model {
        private static readonly PasswordHasher<UserModel> passwordHasher = new PasswordHasher<UserModel>();

        public static string PasswordSalt;

        public Guid Id { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        
        public string Password {
            set {
                PasswordHash = passwordHasher.HashPassword(this, value + PasswordSalt);
            }
        }

        public DateTime? LastLogin { get; set; }
        public Guid? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenDate { get; set; }
        public DateTime? EmailVerificationTokenExpiration { get; set; }

        public virtual IList<UserRoleModel> UserRoleModels { get; set; } = new List<UserRoleModel>();
        public virtual IList<UserTokenModel> UserTokenModels { get; set; } = new List<UserTokenModel>();

        public bool ValidatePassword(string password) {
            PasswordVerificationResult passwordVerificationResult = passwordHasher.VerifyHashedPassword(this, PasswordHash, password + PasswordSalt);
            if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded) {
                Password = password;
            }

            return passwordVerificationResult != PasswordVerificationResult.Failed;
        }
    }
}
