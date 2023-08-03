using Microsoft.AspNetCore.Mvc;
using NLogLibrary.Models;
using System.Diagnostics;

namespace NLogLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            int value1 = 5;
            int value2 = 0;
            int result;
            try
            {
                result = value1 / value2;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }

            _logger.LogInformation("Index sayfası açılıyor.");
            _logger.LogWarning("Warning Error");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}