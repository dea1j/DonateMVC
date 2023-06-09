using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PaymentIntegration.Models;
using PaymentIntegration.Repository;
using PayStack.Net;

namespace PaymentIntegration.Controllers
{
    public class DonateController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly string token;

        private PayStackApi PayStack { get; set; }

        public DonateController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
            token = _configuration["Payment:PaystackSK"];
            PayStack = new PayStackApi(token);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(DonateViewModel donate)
        {
            TransactionInitializeRequest request = new()
            {
                AmountInKobo = donate.Amount * 100,
                Email = donate.Email,
                Reference = GenerateRef().ToString(),
                Currency = "NGN",
                CallbackUrl = "http://localhost:49800"
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                var transaction = new TransactionModel()
                {
                    Amount = donate.Amount,
                    Email = donate.Email,
                    TrxRef = request.Reference,
                    Name = donate.Name
                };
                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

               

                return Redirect(response.Data.AuthorizationUrl);
            }
            ViewData["error"] = response.Message;
            return View();
        }

        [HttpGet]
        public IActionResult Donations()
        {
            var transactions = _context.Transactions.Where(x => x.Status == true).ToList();
            ViewData["transactions"] = transactions;
            return View();
        }

        [HttpGet]
        public IActionResult Verify()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Verify(TransactionModel transactionModel)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(transactionModel.TrxRef);
            if (response.Data.Status == "success")
            {
                var transaction = _context.Transactions.Where(x => x.TrxRef == transactionModel.TrxRef).FirstOrDefault();
                if (transaction != null)
                {
                    transaction.Status = true;
                    _context.Transactions.Update(transaction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Donations");
                }
            }
            ViewData["error"] = response.Data.GatewayResponse;
            return RedirectToAction("Index");
        }

        public static int GenerateRef()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return random.Next(100000000, 999999999);
        }
    }
}