using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly DataContext _context;

        public ContactController(DataContext context)
        {
            _context = context;
        }

        // 1. عرض قائمة الرسائل مع نظام الترقيم (Pagination)
        public async Task<IActionResult> Index(int page = 1)
        {
            // عدد الرسائل في كل صفحة
            const int pageSize = 6;

            // جلب إجمالي عدد الرسائل (لأغراض العداد والترقيم)
            var totalMessages = await _context.Contacts.CountAsync();

            // حساب عدد الصفحات الكلي
            var totalPages = (int)Math.Ceiling(totalMessages / (double)pageSize);

            // التأكد من أن رقم الصفحة لا يتخطى الحدود
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // جلب الرسائل الخاصة بالصفحة الحالية فقط
            var messages = await _context.Contacts
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // تمرير بيانات الترقيم للـ View عبر ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalMessagesCount = totalMessages; // عشان العداد اللي فوق في الصفحة يفضل دقيق

            return View(messages);
        }

        // 2. جلب تفاصيل الرسالة بصيغة JSON
        [HttpGet]
        public async Task<IActionResult> GetMessageDetails(Guid id)
        {
            var message = await _context.Contacts.FindAsync(id);
            if (message == null) return NotFound();

            if (!message.IsRead)
            {
                message.IsRead = true;
                _context.Update(message);
                await _context.SaveChangesAsync();
            }

            return Json(new
            {
                fullName = message.FullName,
                email = message.Email,
                messageContent = message.Message,
                date = message.CreatedAt.ToString("MMMM dd, yyyy h:mm tt"),
                imageUrl = !string.IsNullOrEmpty(message.AttachmentUrl) ? "/uploads/" + message.AttachmentUrl : ""
            });
        }

        // 3. حذف رسالة واحدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var message = await _context.Contacts.FindAsync(id);
            if (message != null)
            {
                _context.Contacts.Remove(message);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple(List<Guid> ids)
        {
            if (ids == null || !ids.Any()) return RedirectToAction(nameof(Index));

            var messagesToDelete = await _context.Contacts
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();

            if (messagesToDelete.Any())
            {
                _context.Contacts.RemoveRange(messagesToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}