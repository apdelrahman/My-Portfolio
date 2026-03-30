using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Core.Entities;
using Core.Interfaces;
using Web.VewModels;
using Web.ViewModels;
using Web.Models;

namespace Web.Controllers
{
    [Authorize]
    public class PortfolioItemsController : Controller
    {
        private readonly IWebHostEnvironment _hosting;
        private readonly IUnitOfWork<PortofolioItem> _portfolio;
        private readonly IUnitOfWork<Owner> _owner;
        private readonly IUnitOfWork<Contact> _contact;

        public PortfolioItemsController(
            IUnitOfWork<PortofolioItem> portfolio,
            IUnitOfWork<Owner> owner,
            IUnitOfWork<Contact> contact,
            IWebHostEnvironment hosting)
        {
            _hosting = hosting;
            _portfolio = portfolio;
            _owner = owner;
            _contact = contact;
        }

        public IActionResult Index(int pageNumber = 1)
        {
            int pageSize = 6;
            var allItemsQuery = _portfolio.Entity.GetAll().AsQueryable();
            int totalItems = allItemsQuery.Count();

            var pagedItems = allItemsQuery
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dashboardData = new DashboardViewModel
            {
                OwenerInfo = _owner.Entity.GetAll().FirstOrDefault(),
                PortfolioItems = pagedItems,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                UnreadMessagesCount = _contact.Entity.GetAll().Count()
            };

            return View(dashboardData);
        }

        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();

            var portfolioItem = _portfolio.Entity.GetAll()
                .AsQueryable()
                .Include(p => p.ProjectImages)
                .FirstOrDefault(x => x.Id == id);

            if (portfolioItem == null) return NotFound();

            return View(portfolioItem);
        }

        public IActionResult Create() => View(new PortfolioViewModel { IsActive = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public IActionResult Create(PortfolioViewModel model)
        {
            if (model.ProjectImages != null && model.ProjectImages.Count > 20)
            {
                ModelState.AddModelError("ProjectImages", "عفواً، الحد الأقصى للصور هو 20 صورة فقط للمشروع الواحد.");
            }

            if (ModelState.IsValid)
            {
                string fileName = model.ImageUrl;

                string uploads = Path.Combine(_hosting.WebRootPath, "img", "portfolio");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (model.File != null)
                {
                    fileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                    string fullPath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        model.File.CopyTo(stream);
                    }
                }

                PortofolioItem item = new PortofolioItem
                {
                    ProjectName = model.ProjectName,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    ImageUrl = fileName,
                    ProjectImages = new List<PortfolioImage>()
                };

                if (model.ProjectImages != null && model.ProjectImages.Count > 0)
                {
                    foreach (var file in model.ProjectImages)
                    {
                        string galleryFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string galleryPath = Path.Combine(uploads, galleryFileName);

                        using (var stream = new FileStream(galleryPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        item.ProjectImages.Add(new PortfolioImage { ImageUrl = galleryFileName });
                    }
                }

                _portfolio.Entity.Insert(item);
                _portfolio.save();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var item = _portfolio.Entity.GetAll()
                .AsQueryable()
                .Include(p => p.ProjectImages)
                .FirstOrDefault(x => x.Id == id);

            if (item == null) return NotFound();

            PortfolioViewModel vm = new PortfolioViewModel
            {
                Id = item.Id,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                ProjectName = item.ProjectName,
                IsActive = item.IsActive,
                ExistingImages = item.ProjectImages.Select(i => i.ImageUrl).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public IActionResult Edit(Guid id, PortfolioViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var itemInDb = _portfolio.Entity.GetAll()
                        .AsQueryable()
                        .Include(p => p.ProjectImages)
                        .FirstOrDefault(x => x.Id == id);

                    if (itemInDb == null) return NotFound();

                    itemInDb.ProjectName = model.ProjectName;
                    itemInDb.Description = model.Description;
                    itemInDb.IsActive = model.IsActive;

                    string uploads = Path.Combine(_hosting.WebRootPath, "img", "portfolio");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    if (model.File != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                        string fullPath = Path.Combine(uploads, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            model.File.CopyTo(stream);
                        }
                        itemInDb.ImageUrl = fileName;
                    }

                    if (model.ProjectImages != null && model.ProjectImages.Count > 0)
                    {
                        foreach (var file in model.ProjectImages)
                        {
                            string galleryFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string galleryPath = Path.Combine(uploads, galleryFileName);
                            using (var stream = new FileStream(galleryPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            itemInDb.ProjectImages.Add(new PortfolioImage { ImageUrl = galleryFileName });
                        }
                    }

                    _portfolio.Entity.Update(itemInDb);
                    _portfolio.save();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PortfolioItemExists(model.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteGalleryImage(string imageName, Guid projectId)
        {
            try
            {
                var item = _portfolio.Entity.GetAll()
                    .AsQueryable()
                    .Include(p => p.ProjectImages)
                    .FirstOrDefault(x => x.Id == projectId);

                if (item == null) return Json(new { success = false, message = "Project not found" });

                var imageToRemove = item.ProjectImages.FirstOrDefault(img => img.ImageUrl == imageName);

                if (imageToRemove != null)
                {
                    string uploads = Path.Combine(_hosting.WebRootPath, "img", "portfolio");
                    string fullPath = Path.Combine(uploads, imageName);

                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

                    item.ProjectImages.Remove(imageToRemove);
                    _portfolio.save();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Image not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var item = _portfolio.Entity.GetAll()
                .AsQueryable()
                .Include(p => p.ProjectImages)
                .FirstOrDefault(x => x.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var item = _portfolio.Entity.GetAll()
                .AsQueryable()
                .Include(p => p.ProjectImages)
                .FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                string uploads = Path.Combine(_hosting.WebRootPath, "img", "portfolio");

                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    string thumbPath = Path.Combine(uploads, item.ImageUrl);
                    if (System.IO.File.Exists(thumbPath)) System.IO.File.Delete(thumbPath);
                }

                if (item.ProjectImages != null)
                {
                    foreach (var img in item.ProjectImages)
                    {
                        string fullPath = Path.Combine(uploads, img.ImageUrl);
                        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                    }
                }

                _portfolio.Entity.Delete(id);
                _portfolio.save();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioItemExists(Guid id)
        {
            return _portfolio.Entity.GetAll().Any(e => e.Id == id);
        }
    }
}