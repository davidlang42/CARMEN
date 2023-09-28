using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class DetailOption
    {
        public string Name { get; }
        public IComparer<Applicant> SortBy { get; }

        public DetailOption(string name, IComparer<Applicant> sort_by)
        {
            Name = name;
            SortBy = sort_by;
        }

        public static DetailOption FromCriteria(Criteria criteria)
        {
            //TODO From Criteria
        }

        public static DetailOption FromTag(Tag tag)
        {
            //TODO From Tag
        }
    }
}
