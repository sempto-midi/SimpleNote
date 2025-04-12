namespace SimpleNote.Models
{
    public class AudioChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Volume { get; set; } = 0.7f;
        public bool IsMuted { get; set; }
    }
}