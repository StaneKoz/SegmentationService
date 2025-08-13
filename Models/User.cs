namespace SegmentationService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Segment> Segments { get; set; } = new List<Segment>();
    }
}
