namespace SimpleNote
{
    public partial class MixerChannel
    {
        public string Name { get; set; }
        public float Volume { get; set; } = 0.8f;
        public bool IsMuted { get; set; }
    }
}