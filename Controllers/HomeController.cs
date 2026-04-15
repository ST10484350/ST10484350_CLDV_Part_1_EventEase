using Microsoft.AspNetCore.Mvc;
using ST10484350_CLDV_Part_1_EventEase.Models;
using System.Diagnostics;

namespace ST10484350_CLDV_Part_1_EventEase.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}