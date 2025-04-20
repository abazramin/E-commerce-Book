using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bullky.DataAccess.Data;
using Bullky.Models.Models;
using Bullky.Repositry.IRepositry;
using Bullky.Utilty;
using Microsoft.AspNetCore.Authorization;

namespace Bullky.Areas.Admin.Controllers
{


    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]

	public class CatogeriesController : Controller
    {
        private readonly IUintOfWork _context;

        public CatogeriesController(IUintOfWork context)
        {
            _context = context;
        }

        // GET: Catogeries
        public  IActionResult Index()
        {
            return View( _context.CatogeryRepositry.GetAll());
        }

        // GET: Catogeries/Details/5
        public  IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catogery = _context.CatogeryRepositry.Get(u => u.ID == id);
              
            if (catogery == null)
            {
                return NotFound();
            }

            return View(catogery);
        }

        // GET: Catogeries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Catogeries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("ID,Name,OrderItem")] Catogery catogery)
        {
            if (ModelState.IsValid)
            {
                _context.CatogeryRepositry.Add(catogery);
                 _context.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(catogery);
        }

        // GET: Catogeries/Edit/5
        public  IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catogery =  _context.CatogeryRepositry.Get(i => i.ID == id);
            if (catogery == null)
            {
                return NotFound();
            }
            return View(catogery);
        }

        // POST: Catogeries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ID,Name,OrderItem")] Catogery catogery)
        {
            if (id != catogery.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.CatogeryRepositry.Update(catogery);
                    _context.Save();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(catogery);
        }

        // GET: Catogeries/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catogery = _context.CatogeryRepositry.Get(i => i.ID == id);
                
            if (catogery == null)
            {
                return NotFound();
            }

            return View(catogery);
        }

        // POST: Catogeries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var catogery =  _context.CatogeryRepositry.Get(i => i.ID ==id);
            if (catogery != null)
            {
                _context.CatogeryRepositry.Remove(catogery);
            }

            _context.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
