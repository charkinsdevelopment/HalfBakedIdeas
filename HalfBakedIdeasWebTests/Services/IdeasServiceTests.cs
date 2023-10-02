using HalfBakedIdeasWeb.Data;
using HalfBakedIdeasWeb.Data.Models;
using HalfBakedIdeasWeb.Models.Requests;
using HalfBakedIdeasWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfBakedIdeasWebTests.ServiceTests
{
    public class IdeasServiceTests
    {
        readonly Mock<ApplicationDbContext> _dbContext;

        public IdeasServiceTests()
        {
            _dbContext = new Mock<ApplicationDbContext>();
        }

        [Fact]
        async Task GetIdeas_ReturnsIdeas()
        {
            _dbContext.Setup(x => x.Ideas)
                .ReturnsDbSet(new List<Idea>()
                {
                    new Idea()
                    {
                        Id = Guid.NewGuid(),
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow,
                        IWouldUseThisVotes = Faker.RandomNumber.Next(1000),
                        IWouldPayForThisVotes = Faker.RandomNumber.Next(1000)
                    }
                });

            var service = new IdeasService(_dbContext.Object);

            var result = await service.GetIdeas();

            Assert.Single(result);
        }

        [Fact]
        async Task GetIdeas_SortByLatest_ReturnsLatest()
        {
            _dbContext.Setup(x => x.Ideas)
                .ReturnsDbSet(new List<Idea>()
                {
                    new Idea()
                    {
                        Id = Guid.Empty,
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow,
                        IWouldUseThisVotes = Faker.RandomNumber.Next(1000),
                        IWouldPayForThisVotes = Faker.RandomNumber.Next(1000)
                    },
                    new Idea()
                    {
                        Id = Guid.NewGuid(),
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow.AddDays(-5),
                        IWouldUseThisVotes = Faker.RandomNumber.Next(1000),
                        IWouldPayForThisVotes = Faker.RandomNumber.Next(1000)
                    }
                });

            var service = new IdeasService(_dbContext.Object);

            var result = await service.GetIdeas(new IdeasQueryRequest()
            {
                SortBy = SortBy.Latest
            });

            //assert we didn't lose any
            Assert.Equal(2, result.Count());
            //assert that first element has the Guid we are looking for, since its date is sooner than element 2.
            Assert.Equal(Guid.Empty, result.First().Id);
        }

        [Fact]
        async Task GetIdeas_SortByBuyThis_ReturnsCorrectOrder()
        {
            _dbContext.Setup(x => x.Ideas)
                .ReturnsDbSet(new List<Idea>()
                {
                    new Idea()
                    {
                        Id = Guid.Empty,
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow,
                        IWouldUseThisVotes = 1,
                        IWouldPayForThisVotes = 2
                    },
                    new Idea()
                    {
                        Id = new Guid("00000000-0000-0000-0000-000000000001"),
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow.AddDays(-5),
                        IWouldUseThisVotes = 1,
                        IWouldPayForThisVotes = 4
                    }
                });

            var service = new IdeasService(_dbContext.Object);

            var result = await service.GetIdeas(new IdeasQueryRequest()
            {
                SortBy = SortBy.IWouldBuyThis
            });

            //assert we didn't lose any
            Assert.Equal(2, result.Count());
            //assert that first element has the Guid we are looking for, since its has more pay for this votes.
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), result.First().Id);
        }

        [Fact]
        async Task GetIdeas_SortByUseThis_ReturnsCorrectOrder()
        {
            _dbContext.Setup(x => x.Ideas)
                .ReturnsDbSet(new List<Idea>()
                {
                    new Idea()
                    {
                        Id = Guid.Empty,
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow,
                        IWouldUseThisVotes = 1,
                        IWouldPayForThisVotes = 2
                    },
                    new Idea()
                    {
                        Id = new Guid("00000000-0000-0000-0000-000000000001"),
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow.AddDays(-5),
                        IWouldUseThisVotes = 4,
                        IWouldPayForThisVotes = 1
                    }
                });

            var service = new IdeasService(_dbContext.Object);

            var result = await service.GetIdeas(new IdeasQueryRequest()
            {
                SortBy = SortBy.IWouldUseThis
            });

            //assert we didn't lose any
            Assert.Equal(2, result.Count());
            //assert that first element has the Guid we are looking for, since its has more pay for this votes.
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), result.First().Id);
        }

        [Fact]
        async Task GetCanVote_ReturnsCannotVoteBuy_WhenUserHasVotedBuy()
        {
            var ideaId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            _dbContext.Setup(x => x.Ideas)
                .ReturnsDbSet(new List<Idea>()
                {
                    new Idea()
                    {
                        Id = ideaId,
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow,
                        IWouldUseThisVotes = 1,
                        IWouldPayForThisVotes = 2
                    },
                    new Idea()
                    {
                        Id = new Guid("00000000-0000-0000-0000-000000000001"),
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow.AddDays(-5),
                        IWouldUseThisVotes = 4,
                        IWouldPayForThisVotes = 1
                    }
                });

            _dbContext.Setup(x => x.UserVotes)
                .ReturnsDbSet(new List<IdeaUserVotes>()
                {
                    new IdeaUserVotes()
                    {
                        Id = Guid.NewGuid(),
                        IdeaId = ideaId,
                        UserId = userId,
                        VoteType = VoteType.IWouldBuyThis
                    }
                });

            var service = new IdeasService(_dbContext.Object);

            var result = await service.GetCanVote(ideaId, userId);

            Assert.True(result.canVoteUse);
            Assert.False(result.canVoteBuy);
        }

        [Fact]
        async Task GetCanVote_ReturnsCannotVoteUse_WhenUserHasVotedUse()
        {
            var ideaId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            _dbContext.Setup(x => x.Ideas)
                .ReturnsDbSet(new List<Idea>()
                {
                    new Idea()
                    {
                        Id = ideaId,
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow,
                        IWouldUseThisVotes = 1,
                        IWouldPayForThisVotes = 2
                    },
                    new Idea()
                    {
                        Id = new Guid("00000000-0000-0000-0000-000000000001"),
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow.AddDays(-5),
                        IWouldUseThisVotes = 4,
                        IWouldPayForThisVotes = 1
                    }
                });

            _dbContext.Setup(x => x.UserVotes)
                .ReturnsDbSet(new List<IdeaUserVotes>()
                {
                    new IdeaUserVotes()
                    {
                        Id = Guid.NewGuid(),
                        IdeaId = ideaId,
                        UserId = userId,
                        VoteType = VoteType.IWouldUseThis
                    }
                });

            var service = new IdeasService(_dbContext.Object);

            var result = await service.GetCanVote(ideaId, userId);

            Assert.False(result.canVoteUse);
            Assert.True(result.canVoteBuy);
        }

        [Fact]
        async Task GetCanVote_ReturnsCannotVote_WhenUserHasVotedBoth()
        {
            var ideaId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            _dbContext.Setup(x => x.Ideas)
                .ReturnsDbSet(new List<Idea>()
                {
                    new Idea()
                    {
                        Id = ideaId,
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow,
                        IWouldUseThisVotes = 1,
                        IWouldPayForThisVotes = 2
                    },
                    new Idea()
                    {
                        Id = new Guid("00000000-0000-0000-0000-000000000001"),
                        Name = Faker.Lorem.Sentence(),
                        Description = string.Join(' ', Faker.Lorem.Sentences(3)),
                        Category = Faker.Enum.Random<Category>(),
                        CreatedOn = DateTime.UtcNow.AddDays(-5),
                        IWouldUseThisVotes = 4,
                        IWouldPayForThisVotes = 1
                    }
                });

            _dbContext.Setup(x => x.UserVotes)
                .ReturnsDbSet(new List<IdeaUserVotes>()
                {
                    new IdeaUserVotes()
                    {
                        Id = Guid.NewGuid(),
                        IdeaId = ideaId,
                        UserId = userId,
                        VoteType = VoteType.IWouldUseThis
                    },
                    new IdeaUserVotes()
                    {
                        Id = Guid.NewGuid(),
                        IdeaId = ideaId,
                        UserId = userId,
                        VoteType = VoteType.IWouldBuyThis
                    }
                });

            var service = new IdeasService(_dbContext.Object);

            var result = await service.GetCanVote(ideaId, userId);

            Assert.False(result.canVoteUse);
            Assert.False(result.canVoteBuy);
        }
    }
}
