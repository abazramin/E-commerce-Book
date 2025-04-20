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
using Bullky.Repositry;


namespace Bullky.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CompanyController : Controller
	{
		private readonly IUintOfWork _context;
		

		public CompanyController(IUintOfWork context, IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
		}

		// GET: Products
		public IActionResult Index()
		{
			return View(_context.CompanyRepositry.GetAll().ToList());
		}

		// GET: Products/Details/5
		public IActionResult Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var company = _context.CompanyRepositry.Get(i => i.Id == id);

			if (company == null)
			{
				return NotFound();
			}

			return View(company);
		}

		// GET: Products/Create
		public IActionResult Upsert(int? id)
		{


			if (id == null || id == 0)
			{
				//create
				return View(new Company());
			}
			else
			{
				//update
				Company companyObj = _context.CompanyRepositry.Get(u => u.Id == id);
				return View(companyObj);
			}

			
		}

		// POST: Products/Create
		// To protect from overposting attacks , enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Upsert(Company obj)
		{
			if (ModelState.IsValid)
			{

				if (obj.Id == 0)
				{
					_context.CompanyRepositry.Add(obj);
				}
				else
				{
					_context.CompanyRepositry.Update(obj);
				}

				_context.Save();
				TempData["success"] = "Company created successfully";
				return RedirectToAction("Index");
			}
			else
			{

				return View(obj);
			}
		}
		// POST: Products/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			var company = _context.CompanyRepositry.Get(i => i.Id == id);
			if (company != null)
			{
				_context.CompanyRepositry.Remove(company);
			}

			_context.Save();
			TempData["message"] = "Prodcut Delete Successfully";
			return RedirectToAction(nameof(Index));
		}


		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Company> obj = _context.CompanyRepositry.GetAll().ToList();

			return Json(new { data = obj });
		}
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var objBeDelete = _context.CompanyRepositry.Get(i => i.Id == id);

			if (objBeDelete == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
		
				
			_context.CompanyRepositry.Remove(objBeDelete);
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