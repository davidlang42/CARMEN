using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    public struct Availability
    {
        public Item[] AlreadyInItems { get; init; }
        public NonMultiSectionItem[] AlreadyInNonMultiSections { get; init; }
        public AdjacentItem[] InAdjacentItems { get; init; }

        public bool IsAvailable => !IsAlreadyInItem && !IsAlreadyInNonMultiSection && !IsInAdjacentItem;
        public bool IsAlreadyInItem => AlreadyInItems?.Length > 0;
        public bool IsAlreadyInNonMultiSection => AlreadyInNonMultiSections?.Length > 0;
        public bool IsInAdjacentItem => InAdjacentItems?.Length > 0;
    }

    public struct NonMultiSectionItem
    {
        public Section NonMultiSection { get; init; }
        public Item AlreadyInItem { get; init; }
    }

    public struct AdjacentItem
    {
        public Item AlreadyInItem { get; init; }
        public Adjacency Adjacency { get; init; }
        public Item AdjacentTo { get; init; }
        public InnerNode NonConsecutiveSection { get; init; }
    }

    public enum Adjacency
    {
        Next,
        Previous
    }
}
