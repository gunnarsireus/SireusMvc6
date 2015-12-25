using Microsoft.AspNet.Mvc;

namespace SireusMvc6.Controllers
{
    public class AlbumsController : Controller
    {
        //
        // GET: /Albums/
        public ActionResult Index()
        {
            return View();
        }
    }
}