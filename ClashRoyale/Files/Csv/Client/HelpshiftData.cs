namespace ClashRoyale.Files.Csv.Client
{
    public class HelpshiftData : CsvData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpshiftData"/> class.
        /// </summary>
        /// <param name="CsvRow"></param>
        /// <param name="CsvTable"></param>
        public HelpshiftData(CsvRow CsvRow, CsvTable CsvTable) : base(CsvRow, CsvTable)
        {
            // HelpshiftData.
        }

        /// <summary>
        /// Called when all instances has been loaded for initialized members in instance.
        /// </summary>
		public override void LoadingFinished()
		{
	    	// LoadingFinished.
		}
	
        public string HelpshiftId
        {
            get; set;
        }

    }
}