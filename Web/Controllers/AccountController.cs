using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Web.Models;
using System.Linq;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "PortfolioItems");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // --- التعديل المؤقت يبدأ من هنا ---
                // بندور على اليوزر بـ UserName اللي هو (berry)
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user != null)
                {
                    // تسجيل دخول مباشر بدون التأكد من PasswordHash المكسور
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "PortfolioItems");
                }
                // --- التعديل المؤقت ينتهي هنا ---

                ModelState.AddModelError(string.Empty, "اسم المستخدم غير صحيح.");
            }

            return View(model);
        }

        // ميثود إنشاء الأدمن - دي اللي هتحل المشكلة للأبد
        [AllowAnonymous]
        public async Task<string> CreateAdmin()
        {
            // 1. تنظيف أي محاولات قديمة
            var oldAdmin = await _userManager.FindByNameAsync("admin");
            if (oldAdmin != null) await _userManager.DeleteAsync(oldAdmin);

            var existingBerry = await _userManager.FindByNameAsync("berry");
            if (existingBerry != null) await _userManager.DeleteAsync(existingBerry);

            // 2. إعداد البيانات الجديدة
            var user = new ApplicationUser
            {
                UserName = "berry",
                FullName = "Abdelrahman",
                Email = "abeernashat140@gmail.com",
                EmailConfirmed = true,
                CreatedAt = System.DateTime.Now, // عشان الجداول اللي بتطلب تاريخ
                IsActive = true
            };

            // 3. إنشاء اليوزر (هنا السيستم هيشفر الباسورد صح 100%)
            var result = await _userManager.CreateAsync(user, "Abeer2582003");

            if (result.Succeeded)
            {
                return "تم إنشاء الحساب بنجاح! 🎉 \n اليوزر نيم: berry \n الباسورد: Abeer2582003 \n اذهب الآن لصفحة Login واكتب berry.";
            }

            return "فشل الإنشاء: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}