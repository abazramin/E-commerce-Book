using System.Diagnostics;
using System.Security.Claims;
using Bullky.Models;
using Bullky.Models.Models;
using Bullky.Repositry.IRepositry;
using Bullky.Utilty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bullky.Areas.Customer.Controllers
{

    [Area("Customer")]
	public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IUintOfWork _uintOfWork;


		public HomeController(ILogger<HomeController> logger , IUintOfWork uintOfWork)
        {
            _logger = logger;
			_uintOfWork = uintOfWork;
		}

        public IActionResult Index()
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			if (claim != null)
			{
				HttpContext.Session.SetInt32(SD.SessionCart, _uintOfWork.ShoppingCardRepositry
				.GetAll(u => u.ApplicationUserId == claim.Value).Count());
			}


			IEnumerable<Product> productList = _uintOfWork.ProductRepositry.GetAll(includePropreites: "Category");

			return View(productList);
		}

		public IActionResult Details(int id)
		{
			if (id != null)
			{
				ShoppingCard shoppingCard = new()
				{
					Product = _uintOfWork.ProductRepositry.Get(i => i.Id == id, includePropreites: "Category"),
					Count = 1,
					ProductId = id,
				};
				return View(shoppingCard);
			}
			else
			{
				return NotFound();
			}
		}

		[HttpPost]
		[Authorize]
		public IActionResult Details(ShoppingCard obj)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			obj.ApplicationUserId = userId;

		

			// check if the product is already in the cart.
			ShoppingCard CartFromDB = _uintOfWork.ShoppingCardRepositry
				.Get(u => u.ApplicationUserId == userId && u.ProductId == obj.ProductId);

			if (CartFromDB != null)
			{
				// update cart
				CartFromDB.Count += obj.Count;
				_uintOfWork.ShoppingCardRepositry.Update(CartFromDB);
				_uintOfWork.Save();
			} else
			{
				// create cart
				_uintOfWork.ShoppingCardRepositry.Add(obj);
				_uintOfWork.Save();
				HttpContext.Session.SetInt32(SD.SessionCart,  _uintOfWork.ShoppingCardRepositry
				.GetAll(u => u.ApplicationUserId == userId).Count());
			}


			TempData["Message"] = "Shopping Card Created Successfully";


			return RedirectToAction(nameof(Index));
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
