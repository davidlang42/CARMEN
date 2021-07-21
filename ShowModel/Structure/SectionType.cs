using System.ComponentModel.DataAnnotations;

namespace ShowModel.Structure
{
    public class SectionType
    {
        [Key]
        public int SectionTypeId { get; private set; }
        public string Name { get; set; } = "Section";
        public virtual Image? Icon { get; set; }
        public bool AllowMultipleRoles { get; set; } = false;
        public bool AllowNoRoles { get; set; } = false;
    }
}
