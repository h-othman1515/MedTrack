using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MedTrack.Models;
using MedTrack.Models.ViewModels;

namespace MedTrack.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HomePageViewModel
            {
                RegisteredPharmacies = 156,
                MedicationsTracked = 12450,
                WastePrevented = 87500,
                ShortagesResolved = 342
            };
            return View(model);
        }

        public IActionResult About() => View();
        public IActionResult Features() => View();
        public IActionResult Privacy() => View();

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
