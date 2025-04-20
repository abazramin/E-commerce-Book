using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web.WebPages.Html;
using Microsoft.EntityFrameworkCore;
using Bullky.DataAccess.Data;
using Bullky.Models.Models;
using Bullky.Repositry.IRepositry;
using Bullky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Bullky.Utilty;
using SelectListItem = Microsoft.AspNetCore.Mvc.Rendering.SelectListItem;


namespace Bullky.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class ProductsController : Controller
	{
		private readonly IUintOfWork _context;
		// Di of webHostEnvironment
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProductsController(IUintOfWork context, IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
		}

		// GET: Products
		public IActionResult Index()
		{
			return View(_context.ProductRepositry.GetAll(includePropreites: "Category").ToList());
		}

		// GET: Products/Details/5
		public IActionResult Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = _context.ProductRepositry.Get(i => i.Id == id);
			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		// GET: Products/Create
		public IActionResult Upsert(int? id)
		{
			ProductVm productVm = new()
			{
				Product = new Product(),
				CategoryList = _context.CatogeryRepositry.GetAll()
				.Select(u => new SelectListItem()
				{
					Text = u.Name,
					Value = u.ID.ToString()
				})
			};

			if (id == null || id == 0)
			{
				// mode is add or create
				return View(productVm);
			}
			else
			{
				// mode is edit or update
				productVm.Product = _context.ProductRepositry.Get(i => i.Id == id);
				return View(productVm);
			}

		
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Upsert(ProductVm obj, IFormFile? file)
		{
			if (ModelState.IsValid)
			{
				// first step : get current path
				string wwwRootPaath = _webHostEnvironment.WebRootPath;
				//
				if (file != null)
				{
					// 
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					// create path for image
					string productPath = Path.Combine(wwwRootPaath, @"Images\Products");


					if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
					{
						// delete old img. 
						string oldImagePath = Path.Combine(wwwRootPaath, obj.Product.ImageUrl.TrimStart('\\'));
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					using (var fileStrim = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStrim);
					}

					obj.Product.ImageUrl = @"\Images\Products\" + fileName;

				}

				// check if product id is 0 or not
				if (obj.Product.Id == 0)
				{
					_context.ProductRepositry.Add(obj.Product);
					TempData["message"] = "Product Created Successfully";

				}
				else
				{
					_context.ProductRepositry.Update(obj.Product);
					TempData["message"] = "Product Edit Successfully";

				}


				_context.Save();
				return RedirectToAction(nameof(Index));
			}
			else
			{
				obj.CategoryList = _context.CatogeryRepositry.GetAll()
						   .Select(u => new SelectListItem()
						   {
							   Text = u.Name,
							   Value = u.ID.ToString()
						   });
			}
			;
			return View(obj);
		}
		// POST: Products/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			var product = _context.ProductRepositry.Get(i => i.Id == id);
			if (product != null)
			{
				_context.ProductRepositry.Remove(product);
			}

			_context.Save();
			TempData["message"] = "Prodcut Delete Successfully";
			return RedirectToAction(nameof(Index));
		}
		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Product> obj = _context.ProductRepositry.GetAll(includePropreites: "Category").ToList();

			return Json(new { data = obj });
		}
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var objBeDelete = _context.ProductRepositry.Get(i => i.Id == id);

			if (objBeDelete == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
			if (!string.IsNullOrEmpty(objBeDelete.ImageUrl))
			{
				// delete old img.
				string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objBeDelete.ImageUrl.TrimStart('\\'));
				if (System.IO.File.Exists(oldImagePath))
				{
					System.IO.File.Delete(oldImagePath);
				}
			}
			_context.ProductRepositry.Remove(objBeDelete);
			_context.Save();

			return Json(new { success = true, message = "Delete Successful" });
		}

		#endregion

		//private bool ProductExists(int id)
		//{
		//    return _context.Products.Any(e => e.Id == id);
		//}
	}
}