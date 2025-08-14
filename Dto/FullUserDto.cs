namespace SegmentationService.Dto
{
    public class FullUserDto
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public List<string> Segments {  get; set; } = new List<string>();
    }
}
