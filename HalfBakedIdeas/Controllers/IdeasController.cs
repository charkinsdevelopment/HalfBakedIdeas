using Microsoft.AspNetCore.Mvc;

namespace HalfBakedIdeas.Controllers
{
    public class IdeasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
