namespace SimpleNote.Models
{
    public class UserSample
    {
        public int UserId { get; set; }
        public int SampleId { get; set; }
        public int UsedInProject { get; set; }

        // Навигационные свойства
        public virtual User User { get; set; }
        public virtual Sample Sample { get; set; }
        public virtual Project Project { get; set; }
    }
}