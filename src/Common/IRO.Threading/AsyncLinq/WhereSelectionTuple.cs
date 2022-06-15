namespace IRO.Threading.AsyncLinq
{
    /// <summary>
    /// Where
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct WhereSelectionTuple<T>
    {
        public T Item { get; set; }

        public bool IsIncluded { get; set; }
    }
}
