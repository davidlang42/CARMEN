using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using ShowModel.Requirements;

namespace ShowModel.Applicants
{
    /// <summary>
    /// The description of an identifier on which applicants can be assigned an identity.
    /// </summary>
    public class Identifier : IOrdered
    {
        [Key]
        public int IdentifierId { get; private set; }
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public string DefaultPrefix { get; set; } = "";
        public string DefaultSuffix { get; set; } = "";
        public virtual ICollection<Requirement> Requirements { get; private set; } = new ObservableCollection<Requirement>();
        public int? AssignNumbersFrom { get; set; } = null;
    }
}
