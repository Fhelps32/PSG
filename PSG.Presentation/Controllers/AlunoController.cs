using Microsoft.AspNetCore.Mvc;

namespace PSG.Presentation.Controllers
{
    public class AlunoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
