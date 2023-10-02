namespace HalfBakedIdeas.Data.Models
{
    public class Idea
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Category Type { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
