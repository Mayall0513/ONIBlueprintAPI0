using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlueprintRepository.Models {
    public sealed class UserRoleModel : Model {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public UserModel User { get; set; }
    }
}
