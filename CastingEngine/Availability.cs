using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
{
    [Flags]
    public enum Availability//LATER add enum unit tests similar to DataObjects
    {
        Available = 0,
        AlreadyInItem = 1,
        AlreadyInNonMultiSection = 2,
        InPreviousItem = 4,
        InNextItem = 8,
        //TODO there will need to be more types of unavailablity due to roles being in multiple items, eg.
        //- already cast in a role which is in THIS item, vs already cast in a role which is in ANOTHER item which this role is in
        //- in next item after THIS item, vs in next item after ANOTHER item which this role is in
        //- " for previous item
        //- already cast in a role which is in a non-multi section containing THIS item, vs already in a role which is in a non-multi section containing ANOTHER item which this role is in
        //
        // *** it would be nice if this enum could be a rust style enum, and return *which* item/section they're already in, or adjacent to, 
        // PROS:
        //- no more enum values required
        //- no need to calc/recalc this based on item, its just about applicant and role
    }
}
