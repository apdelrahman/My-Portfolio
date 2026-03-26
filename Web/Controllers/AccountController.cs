using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Web.Models;
using System.Linq; // ضرورية لعمل الـ Select في رسائل الخطأ

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
            // لو اليوزر مسجل دخول بالفعل، ويديه للداشبورد علطول
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
                // تسجيل الدخول باستخدام UserName (اللي هو هيكون berry)
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "PortfolioItems");
                }

                ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            return View(model);
        }

        // --- ميثود إنشاء الأدمن ببيانات Abeer الجديدة ---
        [AllowAnonymous]
        public async Task<string> CreateAdmin()
        {
            // 1. تنظيف الداتابيز من أي يوزر قديم (admin أو berry) عشان م يحصلش تعارض
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
                EmailConfirmed = true
            };

            // 3. إنشاء اليوزر بالباسورد: Abeer2582003
            var result = await _userManager.CreateAsync(user, "Abeer2582003");

            if (result.Succeeded)
            {
                return "تم إنشاء الحساب بنجاح! 🎉 \n اليوزر نيم: berry \n الباسورد: Abeer2582003 \n\n اذهب الآن لصفحة Login لتسجيل الدخول.";
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