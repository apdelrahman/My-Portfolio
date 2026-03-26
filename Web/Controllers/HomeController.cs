using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.VewModels;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork<Owner> _owner;
        private readonly IUnitOfWork<PortofolioItem> _portfolio;
        private readonly IUnitOfWork<Contact> _contact; 
        private readonly IWebHostEnvironment _hosting; 

        public HomeController(
            IUnitOfWork<Owner> owner,
            IUnitOfWork<PortofolioItem> portfolio,
            IUnitOfWork<Contact> contact,
            IWebHostEnvironment hosting)
        {
            _owner = owner;
            _portfolio = portfolio;
            _contact = contact;
            _hosting = hosting;
        }

        public IActionResult Index()
        {
            var ownerData = _owner.Entity.GetAll().FirstOrDefault();

            if (ownerData == null)
            {
                ownerData = new Owner
                {
                    FullName = "Name Not Set",
                    Profile = "Profile Not Set",
                    Avatar = "default.png"
                };
            }

            var activeItems = _portfolio.Entity.GetAll()
                                .AsQueryable()
                                .Where(x => x.IsActive == true)
                                .Include(x => x.ProjectImages)
                                .ToList();

            var homeViewModel = new HomeViewModel
            {
                Owner = ownerData,
                PortfolioItems = activeItems
            };

            return View(homeViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Contact(string FullName, string Email, string Message, IFormFile file)
        {
            try
            {
                string fileName = null;
                if (file != null)
                {
                    
                    string uploads = Path.Combine(_hosting.WebRootPath, "uploads");
                    fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string fullPath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create)) { await file.CopyToAsync(stream); }
                }

                var newContact = new Contact
                {
                    Id = Guid.NewGuid(),            
                    FullName = FullName,
                    Email = Email,
                    Message = Message,
                    AttachmentUrl = fileName ?? "",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _contact.Entity.Insert(newContact);
                _contact.save();

                TempData["SuccessMessage"] = "Thank you! Your message has been sent.";
            }
            catch (Exception ex)
            {
               
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}