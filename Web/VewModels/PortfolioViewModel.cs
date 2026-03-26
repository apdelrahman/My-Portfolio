using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Web.VewModels
{
    public class PortfolioViewModel
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }

        // الصورة الأساسية (الغلاف)
        public string? ImageUrl { get; set; }
        public IFormFile? File { get; set; }

        // --- شغل الألبوم الجديد ---

        // 1. الخاصية دي اللي هتستقبل الـ 20 صورة من الفورم (رفع جديد)
        public List<IFormFile>? ProjectImages { get; set; }

        // 2. الخاصية دي هنستخدمها في الـ Edit عشان نعرض الصور اللي موجودة فعلاً في الداتابيز
        public List<string>? ExistingImages { get; set; }

        public bool IsActive { get; set; }
    }
}