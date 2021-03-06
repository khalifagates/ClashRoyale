namespace ClashRoyale.Files.Csv.Client
{
    public class CreditData : CsvData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CreditData" /> class.
        /// </summary>
        /// <param name="CsvRow">The row.</param>
        /// <param name="CsvTable">The data table.</param>
        public CreditData(CsvRow CsvRow, CsvTable CsvTable) : base(CsvRow, CsvTable)
        {
            // CreditData.
        }

        /// <summary>
        ///     Called when all instances has been loaded for initialized members in instance.
        /// </summary>
        public override void LoadingFinished()
        {
            // LoadingFinished.
        }
    }
}