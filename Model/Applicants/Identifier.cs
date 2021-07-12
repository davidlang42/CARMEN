using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Model.Requirements;

namespace Model.Applicants
{
    /// <summary>
    /// The description of an identifier on which applicants can be assigned an identity.
    /// </summary>
    public class Identifier
    {
        [Key]
        public int IdentifierId { get; private set; }
        public string Name { get; set; } = "";
        public string DefaultPrefix { get; set; } = "";
        public string DefaultSuffix { get; set; } = "";
        public virtual ICollection<Requirement> Requirements { get; private set; } = new ObservableCollection<Requirement>();
        public int? AssignNumbersFrom { get; set; } = null;
    }
}
