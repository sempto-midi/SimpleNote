namespace SimpleNote.Models
{
    public class Sample
    {
        public int SampleId { get; set; }
        public string SampleName { get; set; }
        public string SampleData { get; set; }
        public int UploadedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Коллекция связанных UserSample
        public virtual ICollection<UserSample> UserSamples { get; set; } = new List<UserSample>();
    }
}