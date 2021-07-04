﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
{
    /// <summary>
    /// A group of people which an applicant can be cast into.
    /// </summary>
    public class CastGroup : IOrdered
    {
        [Key]
        public int GroupId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public virtual ICollection<Applicant> Members { get; private set; } = new ObservableCollection<Applicant>();
    }
}
