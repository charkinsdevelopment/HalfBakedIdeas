using HalfBakedIdeasWeb.Data;
using HalfBakedIdeasWeb.Data.Models;
using HalfBakedIdeasWeb.Models;
using HalfBakedIdeasWeb.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace HalfBakedIdeasWeb.Services
{
    public interface IIdeasService
    {
        Task<IEnumerable<Idea>> GetIdeas();
        Task<IEnumerable<Idea>> GetIdeas(IdeasQueryRequest query);
        Task<(bool canVoteUse, bool canVoteBuy)> GetCanVote(Guid ideaId, string userId);
        Task<(bool voted, int count)> Vote(Guid ideaId, string userId, VoteType voteType);
        Task<(bool voted, int count)> UnVote(Guid ideaId, string userId, VoteType voteType);
        Task SaveIdea(SubmitIdeaViewModel submitViewModel);
    }
    public class IdeasService : IIdeasService
    {
        readonly ApplicationDbContext _context;

        public IdeasService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Idea>> GetIdeas()
        {
            return await _context.Ideas.ToListAsync();
        }

        public async Task<IEnumerable<Idea>> GetIdeas(IdeasQueryRequest query)
        {
            var ideas = new List<Idea>();

            if (query.Page > 1)
            {
                ideas = await _context.Ideas.Skip(query.Page * query.Limit).ToListAsync();
            }
            else
            {
                ideas = await _context.Ideas.ToListAsync();
            }
                        

            if (query.SortBy == SortBy.Latest)
            {
                ideas = ideas.OrderByDescending(x => x.CreatedOn).ToList();
            }
            else if (query.SortBy == SortBy.IWouldBuyThis)
            {
                ideas = ideas.OrderByDescending(x => x.IWouldPayForThisVotes).ToList();
            }
            else if (query.SortBy == SortBy.IWouldUseThis)
            {
                ideas = ideas.OrderByDescending(x => x.IWouldUseThisVotes).ToList();
            }

            if (query.FilterBy != Category.None)
            {
                ideas = ideas.Where(i => i.Category == query.FilterBy).ToList();
            }

            return ideas;
        }

        public async Task<(bool canVoteUse, bool canVoteBuy)> GetCanVote(Guid ideaId, string userId)
        {
            var idea = await _context.UserVotes
                .Include(i => i.Idea)
                .Where(i => i.IdeaId == ideaId).ToListAsync();

            if (idea == null)
            {
                return (false, false);
            }

            var canVoteUse = !idea.Any(x => x.UserId == userId && x.VoteType == VoteType.IWouldUseThis);
            var canVotePay = !idea.Any(x => x.UserId == userId && x.VoteType == VoteType.IWouldBuyThis);

            return (canVoteUse, canVotePay);
        }

        public async Task<(bool voted, int count)> Vote(Guid ideaId, string userId, VoteType voteType)
        {
            var idea = await _context.Ideas.FindAsync(ideaId);

            if (idea == null)
            {
                return (false, 0);
            }

            if (voteType == VoteType.IWouldBuyThis)
            {
                idea.IWouldPayForThisVotes = idea.IWouldPayForThisVotes + 1;
            }

            if (voteType == VoteType.IWouldUseThis)
            {
                idea.IWouldUseThisVotes = idea.IWouldUseThisVotes + 1;
            }

            var userVote = new IdeaUserVotes()
            {
                IdeaId = ideaId,
                UserId = userId,
                VoteType = voteType
            };

            await _context.UserVotes.AddAsync(userVote);

            await _context.SaveChangesAsync();

            return (true, voteType == VoteType.IWouldBuyThis ? idea.IWouldPayForThisVotes : idea.IWouldUseThisVotes);
        }

        public async Task<(bool voted, int count)> UnVote(Guid ideaId, string userId, VoteType voteType)
        {
            var idea = await _context.Ideas.FindAsync(ideaId);

            if (idea == null)
            {
                return (false, 0);
            }

            if (voteType == VoteType.IWouldBuyThis)
            {
                if(idea.IWouldPayForThisVotes > 0)
                {
                    idea.IWouldPayForThisVotes = idea.IWouldPayForThisVotes - 1;
                }
            }

            if (voteType == VoteType.IWouldUseThis)
            {
                if(idea.IWouldUseThisVotes > 0)
                {
                    idea.IWouldUseThisVotes = idea.IWouldUseThisVotes - 1;
                }
            }

            var existingVote = await _context.UserVotes.FirstOrDefaultAsync(x => x.UserId == userId && x.IdeaId == ideaId && x.VoteType == voteType);
            if (existingVote != null)
            {
                _context.UserVotes.Remove(existingVote);
            }

            await _context.SaveChangesAsync();

            return (true, voteType == VoteType.IWouldBuyThis ? idea.IWouldPayForThisVotes : idea.IWouldUseThisVotes);
        }

        public async Task SaveIdea(SubmitIdeaViewModel submitViewModel)
        {
            var exists = await _context.Ideas.AnyAsync(x => x.Name == submitViewModel.Title || x.Description == submitViewModel.Description);

            if (exists) 
                return;

            var idea = new Idea()
            {
                Name = submitViewModel.Title,
                Description = submitViewModel.Description,
                Category = Category.None,
                Id = new Guid(),
                CreatedOn = DateTime.Now,
                IWouldPayForThisVotes = 0,
                IWouldUseThisVotes = 0
            };

            await _context.Ideas.AddAsync(idea);

            await _context.SaveChangesAsync();
        }
    }
}
