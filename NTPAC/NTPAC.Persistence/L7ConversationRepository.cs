using NTPAC.Common.Interfaces.Persistence;
using NTPAC.Common.Models;

namespace NTPAC.Persistence
{
    public class L7ConversationRepository : UserRepositoryBase<L7Conversation>
    {
        public L7ConversationRepository(IRepository<L7Conversation> repositoryImplmenetation) : base(repositoryImplmenetation) { }
    }
}
