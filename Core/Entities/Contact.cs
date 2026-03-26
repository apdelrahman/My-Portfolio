using System;

namespace Core.Entities
{
    public class Contact : Entitybase
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string AttachmentUrl { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}