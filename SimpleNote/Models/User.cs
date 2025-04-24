namespace SimpleNote.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; } // Новое поле

        public User()
        {
            CreatedAt = DateTime.Now;
        }

        // Коллекция связанных UserSample
        public virtual ICollection<UserSample> UserSamples { get; set; } = new List<UserSample>();
    }
}