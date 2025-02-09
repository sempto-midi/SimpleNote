namespace SimpleNote.Models
{
    public class Measure
    {
        public int Id { get; set; } // Первичный ключ

        public int TrackId { get; set; } // Связь с дорожкой (Track)
        public virtual Track Track { get; set; } // Навигационное свойство для дорожки

        public int MeasureNumber { get; set; } // Номер такта

        public virtual ICollection<Note> Notes { get; set; } = new List<Note>(); // Список нот в такте
    }
}