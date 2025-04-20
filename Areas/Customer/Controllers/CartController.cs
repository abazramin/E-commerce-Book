using Bullky.Models.Models;
using Bullky.Models.ViewModels;
using Bullky.Repositry;
using Bullky.Repositry.IRepositry;
using Bullky.Utilty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bullky.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{

		private readonly IUintOfWork _context;
		[BindProperty]
		public ShoppingCartVm ShoppingCartVm { get; set; }

		public CartController(IUintOfWork context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVm shoppingCartVm = new()
			{
				ShoppingCartList = _context.ShoppingCardRepositry
				.GetAll(u => u.ApplicationUserId == userId, includePropreites: "Product"),
				OrderHeader = new()
			};


			foreach (var cart in shoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				shoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(shoppingCartVm);
		}

		public IActionResult Plus(int cartId)
		{
			var cartFromDb = _context.ShoppingCardRepositry.Get(u => u.ShoppingCardID == cartId);
			cartFromDb.Count += 1;
			_context.ShoppingCardRepositry.Update(cartFromDb);
			_context.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cartFromDb = _context.ShoppingCardRepositry.Get(u => u.ShoppingCardID == cartId);
			if (cartFromDb.Count <= 1)
			{
				//remove that from cart

				_context.ShoppingCardRepositry.Remove(cartFromDb);
				HttpContext.Session.SetInt32(SD.SessionCart, _context.ShoppingCardRepositry.
					GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
			}
			else
			{
				cartFromDb.Count -= 1;
				_context.ShoppingCardRepositry.Update(cartFromDb);
			}

			_context.Save();
			return RedirectToAction(nameof(Index));
		}


		public IActionResult Remove(int cartId)
		{
			var cartFromDb = _context.ShoppingCardRepositry.Get(u => u.ShoppingCardID == cartId);

			_context.ShoppingCardRepositry.Remove(cartFromDb);

			HttpContext.Session.SetInt32(SD.SessionCart, _context.ShoppingCardRepositry
			  .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
			_context.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Summery()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVm = new()
			{
				ShoppingCartList = _context.ShoppingCardRepositry.GetAll(u => u.ApplicationUserId == userId,
				includePropreites: "Product"),
				OrderHeader = new()
			};
			// asign the appication user with the order header
			ShoppingCartVm.OrderHeader.ApplicationUser = _context.ApplicationUserRepositry.Get(u => u.Id == userId);

			ShoppingCartVm.OrderHeader.Name = ShoppingCartVm.OrderHeader.ApplicationUser.Name;
			ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVm.OrderHeader.StreetAddress = ShoppingCartVm.OrderHeader.ApplicationUser.StretAddress;
			ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.City;
			ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.State;
			ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.PostelCode;

			foreach (var cart in ShoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}


			return View(ShoppingCartVm);
		}

		[HttpPost]
		[ActionName("Summery")]
		public IActionResult Summery(ShoppingCard shoppingCard)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


			if (ModelState.IsValid)
			{


				ShoppingCartVm.ShoppingCartList = _context.ShoppingCardRepositry.GetAll(u => u.ApplicationUserId == userId,
					includePropreites: "Product");

				// asign the appication user with the order header
				ShoppingCartVm.OrderHeader.OrderDate = DateTime.Now;
				ShoppingCartVm.OrderHeader.ApplicationUserId = userId;

				// asign the appication user with the order header
				ApplicationUser applicationUser = _context.ApplicationUserRepositry.Get(u => u.Id == userId);



				if (applicationUser.CompanyId.GetValueOrDefault() == 0)
				{
					//it is a regular customer 
					ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
					ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusPending;
				}
				else
				{
					//it is a company customer 
					ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
					ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusApproved;
				}


				_context.OrderHeaderRepositry.Add(ShoppingCartVm.OrderHeader);
				_context.Save();
				foreach (var cart in ShoppingCartVm.ShoppingCartList)
				{
					OrderDetail orderDetail = new()
					{
						ProductId = cart.ProductId,
						OrderHeaderId = ShoppingCartVm.OrderHeader.OrderHeaderID,
						Price = cart.Price,
						Count = cart.Count
					};
					_context.OrderDetailsRepositry.Add(orderDetail);
					_context.Save();
				}


				foreach (var cart in ShoppingCartVm.ShoppingCartList)
				{
					cart.Price = GetPriceBasedOnQuantity(cart);
					ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
				}

				if (applicationUser.CompanyId.GetValueOrDefault() == 0)
				{
				}

			}
				return RedirectToAction(nameof(orderConfim) , new { id = ShoppingCartVm.OrderHeader.OrderHeaderID });
		}


		public IActionResult orderConfim(int id)
		{
			HttpContext.Session.Clear();	

			return View(id);
		}

		private double GetPriceBasedOnQuantity(ShoppingCard shoppingCart)
		{
			if (shoppingCart.Count <= 50)
			{
				return shoppingCart.Product.Price;
			}
			else
			{
				if (shoppingCart.Count <= 100)
				{
					return shoppingCart.Product.Price50;
				}
				else
				{
					return shoppingCart.Product.Price100;
				}
			}
		}


	}
}