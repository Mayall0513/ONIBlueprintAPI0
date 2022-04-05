using BlueprintRepository.Models;

namespace BlueprintRepository.Repositories {
    public abstract class AUserTokenRepository : Repository<UserTokenModel> {
        public AUserTokenRepository(BlueprintsDbContext blueprintsContext) : base(blueprintsContext) { }
    }

    public class UserTokenRepository : AUserTokenRepository {
        public UserTokenRepository(BlueprintsDbContext blueprintsContext) : base(blueprintsContext) { }
    }
}
