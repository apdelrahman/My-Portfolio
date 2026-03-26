namespace Core.Entities
{
    public class Owner : Entitybase
    {
        public string FullName { get; set; }
        public string Profile { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; }
        public Address Address { get; set; }

        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string GithubUrl { get; set; }
        public string BehanceUrl { get; set; }
        public string CanvaUrl { get; set; }

        public string AboutTitle { get; set; } 
        public string AboutDescription { get; set; } 
        public string CvUrl { get; set; }   
    }
}
