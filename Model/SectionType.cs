﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
{
    public class SectionType
    {
        #region Database fields
        [Key]
        public int SectionTypeId { get; private set; }
        public string Name { get; set; }
        #endregion

        public SectionType(string name = "")
        {
            Name = name;
        }

        public static SectionType CreateDefault() => new SectionType("Section");
    }
}
