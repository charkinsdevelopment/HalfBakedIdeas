using HalfBakedIdeasWeb.Data.Models;

namespace HalfBakedIdeasWeb.Models.Requests
{
    public class IdeasQueryRequest
    {
        public SortBy SortBy { get; set; }
        public Category FilterBy { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; } = 30;

        public string QueryAsString => $"/ideas/feed?SortBy={SortBy}&Page={Page}";
        public string GoBackQueryAsString => $"/ideas/feed?SortBy={SortBy}&Page={Page - 1}";
        public string GoFowardQueryAsString => $"/ideas/feed?SortBy={SortBy}&Page={Page + 1}";
    }
}
