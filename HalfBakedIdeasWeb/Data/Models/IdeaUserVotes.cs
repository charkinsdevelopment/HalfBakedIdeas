using Microsoft.AspNetCore.Identity;

namespace HalfBakedIdeasWeb.Data.Models
{
    public class IdeaUserVotes
    {
        public Guid Id { get; set; }
        public Guid IdeaId { get; set; }
        public Idea Idea { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public VoteType VoteType { get; set; }
    }
}
