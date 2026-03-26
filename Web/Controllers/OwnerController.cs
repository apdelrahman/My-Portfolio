using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
    [Authorize]
    public class OwnerController : Controller
    {
        private readonly IUnitOfWork<Owner> _owener;
        private readonly IHostingEnvironment _hosting;

        public OwnerController(IUnitOfWork<Owner> owener, IHostingEnvironment hosting)
        {
            _owener = owener;
            _hosting = hosting;
        }

        public IActionResult Edit()
        {
            var item = _owener.Entity.GetAll().FirstOrDefault();
            return View(item ?? new Owner());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // أضفنا deleteAvatar لاستقبال إشارة مسح الصورة من الـ View
        public IActionResult Edit(Owner model, IFormFile File, IFormFile cvFile, bool deleteCv, bool deleteAvatar)
        {
            if (ModelState.IsValid)
            {
                // 1. معالجة الصورة الشخصية (Avatar)
                if (File != null) // لو رفع صورة جديدة
                {
                    string uploads = Path.Combine(_hosting.WebRootPath, "img");
                    string fileName = Guid.NewGuid().ToString() + "_" + File.FileName;
                    string fullPath = Path.Combine(uploads, fileName);

                    // مسح الصورة القديمة من السيرفر قبل استبدالها (اختياري لتحسين المساحة)
                    if (!string.IsNullOrEmpty(model.Avatar))
                    {
                        string oldPath = Path.Combine(uploads, model.Avatar);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        File.CopyTo(stream);
                    }
                    model.Avatar = fileName;
                }
                else if (deleteAvatar) // لو اليوزر داس على زرار Trash للصورة
                {
                    if (!string.IsNullOrEmpty(model.Avatar))
                    {
                        string oldPath = Path.Combine(_hosting.WebRootPath, "img", model.Avatar);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    model.Avatar = null; // سيتم عرض الصورة الافتراضية في الـ View
                }

                // 2. معالجة ملف الـ CV (PDF)
                if (cvFile != null)
                {
                    string uploads = Path.Combine(_hosting.WebRootPath, "files");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    string fileName = Guid.NewGuid().ToString() + "_" + cvFile.FileName;
                    string fullPath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        cvFile.CopyTo(stream);
                    }
                    model.CvUrl = fileName;
                }
                else if (deleteCv)
                {
                    if (!string.IsNullOrEmpty(model.CvUrl))
                    {
                        string oldPath = Path.Combine(_hosting.WebRootPath, "files", model.CvUrl);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    model.CvUrl = null;
                }

                // 3. حفظ البيانات (Insert or Update)
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                    _owener.Entity.Insert(model);
                }
                else
                {
                    _owener.Entity.Update(model);
                }

                _owener.save();

                return RedirectToAction("Index", "PortfolioItems");
            }
            return View(model);
        }
    }
}