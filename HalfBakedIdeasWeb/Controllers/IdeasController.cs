using HalfBakedIdeasWeb.Data;
using HalfBakedIdeasWeb.Data.Models;
using HalfBakedIdeasWeb.Models;
using HalfBakedIdeasWeb.Models.Requests;
using HalfBakedIdeasWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HalfBakedIdeasWeb.Controllers
{
    [Authorize]
    public class IdeasController : Controller
    {
        readonly IIdeasService _ideasService;
        readonly UserManager<IdentityUser> _userManager;
        readonly IdentityUser _currentUser;

        public IdeasController(
            IIdeasService ideasService,
            UserManager<IdentityUser> userManager)
        {
            _ideasService = ideasService;
            _userManager = userManager;
        }

        [Route("ideas/feed")]
        public async Task<IActionResult> Index(IdeasQueryRequest query)
        {
            var ideas = await _ideasService.GetIdeas(query);

            var vm = new IdeaFeedViewModel(ideas);

            if(query.Page <= 1)
            {
                vm.CanGoBackPage = false;
                vm.CanGoForwardPage = true;
            }

            if(query.Page > 1)
            {
                vm.CanGoForwardPage = true;
                vm.CanGoBackPage = true;
            }

            vm.CurrentPage = query.Page == 0 ? 1 : query.Page;
            vm.Query = query.QueryAsString;
            vm.GoBackQuery = query.GoBackQueryAsString;
            vm.GoFowardQuery = query.GoFowardQueryAsString;

            return View("Index", vm);
        }

        [HttpPost]
        [Route("ideas/vote/{ideaId}/{voteType}")]
        public async Task<IActionResult> Vote(
            [FromRoute] Guid ideaId,
            [FromRoute] VoteType voteType)
        {
            var user = await GetUser();

            if(user is null)
            {
                return BadRequest("You're not logged in");
            }

            var canVote = await _ideasService.GetCanVote(ideaId, user.Id);

            if(canVote.canVoteUse && voteType == VoteType.IWouldUseThis)
            {
                var result = await _ideasService.Vote(ideaId, user.Id, voteType);
                return Ok(new VoteResponse(Voted: result.voted, Count: result.count));
            } 
            else if (!canVote.canVoteUse && voteType == VoteType.IWouldUseThis)
            {
                var result = await _ideasService.UnVote(ideaId, user.Id, voteType);
                return Ok(new VoteResponse(Voted: result.voted, Count: result.count));
            }

            if (canVote.canVoteBuy && voteType == VoteType.IWouldBuyThis)
            {
                var result = await _ideasService.Vote(ideaId, user.Id, voteType);
                return Ok(new VoteResponse(Voted: result.voted, Count: result.count));
            }
            else if (!canVote.canVoteBuy && voteType == VoteType.IWouldBuyThis)
            {
                var result = await _ideasService.UnVote(ideaId, user.Id, voteType);
                return Ok(new VoteResponse(Voted: result.voted, Count: result.count));
            }

            return Ok(new VoteResponse(Voted: false, Count: 0));
        }

        [HttpGet]
        public async Task<ActionResult> Submit()
        {
            return View(new SubmitIdeaViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> Submit(SubmitIdeaViewModel submitViewModel)
        {
            var isInvalid = false;

            if(string.IsNullOrWhiteSpace(submitViewModel.Title))
            {
                ViewBag.message = $"Invalid title.";
                isInvalid = true;
            }

            if(string.IsNullOrWhiteSpace(submitViewModel.Description))
            {
                ViewBag.message = $"Invalid description.";
                isInvalid = true;
            }

            if(isInvalid)
                return View(submitViewModel);

            await _ideasService.SaveIdea(submitViewModel);

            ViewBag.message = $"Idea - {submitViewModel.Title} - submitted!";
            return View(new SubmitIdeaViewModel());
        }

        private async Task<IdentityUser> GetUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            return user;
        }
    }
}
