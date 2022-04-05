using BlueprintRepository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlueprintRepository.Repositories {
    public abstract class AUserRepository : Repository<UserModel> {
        public AUserRepository(BlueprintsDbContext blueprintsContext) : base(blueprintsContext) { }

        public abstract void RegisterAsync(UserModel userModel);
    }

    public class UserRepository : AUserRepository {
        public UserRepository(BlueprintsDbContext blueprintsContext) : base(blueprintsContext) { }

        public override async void RegisterAsync(UserModel userModel) {
            if (userModel == null) {
                return;
            }

            Create(userModel);
            await SaveAsync(default);
        }
    }
}
