using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal interface IApplicantField
    {
        string Label { get; }
        public object? Value { get; set; }
    }
}
