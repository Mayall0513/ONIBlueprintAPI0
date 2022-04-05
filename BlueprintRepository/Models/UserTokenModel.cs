using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlueprintRepository.Models {
    public sealed class UserTokenModel : Model, IComparable<UserTokenModel> {
        public Guid UserId { get; set; }
        public Guid TokenId { get; set; }

        public DateTime TokenExpirationDate { get; set; }

        public UserModel User { get; set; }

        public bool Invalidate() {
            return TokenExpirationDate <= DateTime.UtcNow;
        }

        public int CompareTo(UserTokenModel otherToken) {
            return TokenExpirationDate > otherToken.TokenExpirationDate ? -1 : 1;
        }
    }
}
