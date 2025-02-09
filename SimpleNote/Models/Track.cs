namespace SimpleNote.Models
{
    public class Track
    {
        public int TrackId { get; set; } // Первичный ключ
        public int ProjectId { get; set; } // Связь с проектом
        public string TrackName { get; set; } // Имя дорожки
        public string MidiData { get; set; } // Данные MIDI
        public DateTime CreatedAt { get; set; } // Дата создания
        public DateTime UpdatedAt { get; set; } // Дата обновления

        // Навигационное свойство для проекта
        public virtual Project Project { get; set; }

        // Коллекция связанных Measures
        public virtual ICollection<Measure> Measures { get; set; } = new List<Measure>();
    }
}