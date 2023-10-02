using HalfBakedIdeasWeb.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HalfBakedIdeasWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext()
        {

        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Idea> Ideas { get; set; }
        public virtual DbSet<IdeaUserVotes> UserVotes { get; set; }
    }
}