using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class SectionType
    {
        public int SectionTypeId { get; private set; }
        public string Name { get; set; } = "";
        public Show Show { get; set; } = null!;
    }
}
