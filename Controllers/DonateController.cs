using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentIntegration.Models;

namespace PaymentIntegration.Controllers
{
    public class DonateController : Controller
    {
        public DonateController()
        {

        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(DonateViewModel donate)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Donations()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Verify()
        {
            return View();
        }
    }
}