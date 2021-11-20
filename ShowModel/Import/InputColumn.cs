namespace Carmen.ShowModel.Import
{
    public class InputColumn
    {
        public int Index { get; set; }
        public string Header { get; set; }

        public InputColumn(int index, string header)
        {
            Index = index;
            Header = header;
        }

        public override string ToString() => $"{Index}: {Header}";
    }
}
