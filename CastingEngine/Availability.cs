using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
{
    public struct Availability
    {
        public Item[]? AlreadyInItems { get; init; }
        public (Section NonMultiSection, Item AlreadyInItem)[]? AlreadyInNonMultiSections { get; init; }
        public (Item AlreadyInItem, Adjacency Adjacency, Item AdjacentTo)[]? InAdjacentItems { get; init; }

        public bool IsAvailable => !IsAlreadyInItem && !IsAlreadyInNonMultiSection && !IsInAdjacentItem;
        public bool IsAlreadyInItem => AlreadyInItems?.Length > 0;
        public bool IsAlreadyInNonMultiSection => AlreadyInNonMultiSections?.Length > 0;
        public bool IsInAdjacentItem => InAdjacentItems?.Length > 0;
    }

    public enum Adjacency
    {
        Next,
        Previous
    }
}
