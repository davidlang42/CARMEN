using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace Model.Structure
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
