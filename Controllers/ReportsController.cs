using Microsoft.AspNetCore.Mvc;
using MedTrack.Models.ViewModels;

namespace MedTrack.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            var model = new WasteReportViewModel
            {
                PeriodStart = DateTime.Now.AddMonths(-1),
                PeriodEnd = DateTime.Now,
                TotalWasteValue = 3200,
                TotalItemsWasted = 28,
                Items = new List<WasteItemViewModel>()
            };
            return View(model);
        }
    }
}
