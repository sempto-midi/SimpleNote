namespace SimpleNote.Models
{
    public class Note
    {
        public int Id { get; set; } // Первичный ключ

        public int MeasureId { get; set; } // Связь с тактом (Measure)
        public virtual Measure Measure { get; set; } // Навигационное свойство для такта

        public int Pitch { get; set; } // Высота тона (например, MIDI номер ноты)
        public double Duration { get; set; } // Длительность ноты
        public int Velocity { get; set; } // Сила нажатия (MIDI velocity)
    }
}