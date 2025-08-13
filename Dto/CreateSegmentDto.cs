using System.ComponentModel.DataAnnotations;

namespace SegmentationService.Dto
{
    public class CreateSegmentDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }
}
