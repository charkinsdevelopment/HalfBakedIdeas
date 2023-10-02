namespace HalfBakedIdeasWeb.Data.Models
{
    public class Idea
    {
        public Guid Id {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Category Category { get; set; }
        public DateTime CreatedOn { get; set; }
        public int IWouldUseThisVotes { get; set; }
        public int IWouldPayForThisVotes { get; set; }
    }
}
