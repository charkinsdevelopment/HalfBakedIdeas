using HalfBakedIdeasWeb.Data.Models;

namespace HalfBakedIdeasWeb.Models
{
    public class IdeaViewModel
    {
        public IdeaViewModel(Idea idea) {
            Id = idea.Id;
            Name = idea.Name;
            Description = idea.Description;
            Category = idea.Category;
            CreatedOn = idea.CreatedOn;
            IWouldUseThisVotes = idea.IWouldUseThisVotes;
            IWouldPayForThisVotes = idea.IWouldPayForThisVotes;
            UserCanVotePay = false;
            UserCanVoteUse = false;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Category Category { get; set; }
        public DateTime CreatedOn { get; set; }
        public int IWouldUseThisVotes { get; set; }
        public int IWouldPayForThisVotes { get; set; }
        public bool UserCanVotePay { get; set; }
        public bool UserCanVoteUse { get; set; }
    }

    public class IdeaFeedViewModel
    {
        public IdeaFeedViewModel(IEnumerable<Idea> ideas)
        {
            Ideas = ideas.Select(i => new IdeaViewModel(i)).ToList();
        }

        public List<IdeaViewModel> Ideas { get; set; } = new List<IdeaViewModel>();
        public bool CanGoBackPage { get; set; } = false;
        public bool CanGoForwardPage { get; set; } = true;
        public int CurrentPage { get; set; } = 1;
        public string Query { get; set; }
        public string GoBackQuery { get; set; }
        public string GoFowardQuery { get; set; }
    }
}
