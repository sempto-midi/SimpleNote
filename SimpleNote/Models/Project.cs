namespace SimpleNote.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public string ProjectName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Навигационное свойство для пользователя
        public virtual User User { get; set; }

        // Коллекция связанных Tracks
        public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

        // Коллекция связанных UserSample
        public virtual ICollection<UserSample> UserSamples { get; set; } = new List<UserSample>();
    }
}