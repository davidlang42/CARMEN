using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    /// <summary>
    /// A simplier version of System.ComponentModel.DataAnnotations.IValidateableObject
    /// </summary>
    public interface IValidatable
    {
        /// <summary>Checks internal rules to determine if this object is Valid. Any issues are
        /// returned as a sequence of strings. If the sequence is empty, the object is Valid.</summary>
        public IEnumerable<string> Validate();
    }

    public static class IValidatableExtensions
    {
        public static bool IsValid(this IValidatable obj) => !obj.Validate().Any();
    }
}
