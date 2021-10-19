namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A specified assignment of a boolean value to a variable
    /// </summary>
    public struct Assignment<T>
    {
        public T Variable { get; set; }
        public bool? Value { get; set; }

        public override string ToString() => $"{Variable} <- {Value}";
    }
}
