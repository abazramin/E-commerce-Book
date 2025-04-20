using Bullky.Repositry.IRepositry;
using Bullky.Utilty;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bullky.ViewComponets
{
	public class ShoppingCardViewComponet : ViewComponent
	{
		private readonly IUintOfWork _context;
		public ShoppingCardViewComponet(IUintOfWork context)
		{
			_context = context;
		}


		public async Task<IViewComponentResult> InvokeAsync()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			if (claim != null)
			{
				var cartCount = _context.ShoppingCardRepositry
				.GetAll(u => u.ApplicationUserId == claim.Value).Count();
				return View(HttpContext.Session.GetInt32(SD.SessionCart));
			} else
			{
				HttpContext.Session.Clear();
				return View(0);
			}
		



			//var cartCount = _context.ShoppingCardRepositry
			return View();
		}
	}

}
