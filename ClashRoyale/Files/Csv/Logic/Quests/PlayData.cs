namespace ClashRoyale.Files.Csv.Logic.Quests
{
    public class PlayData : CsvData
    {
		/// <summary>
        /// Initializes a new instance of the <see cref="PlayData"/> class.
        /// </summary>
        /// <param name="CsvRow">The row.</param>
        /// <param name="CsvTable">The data table.</param>
        public PlayData(CsvRow CsvRow, CsvTable CsvTable) : base(CsvRow, CsvTable)
        {
            // PlayData.
        }

        /// <summary>
        /// Called when all instances has been loaded for initialized members in instance.
        /// </summary>
		public override void LoadingFinished()
		{
	    	// LoadingFinished.
		}
	
        public int Size
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        public string Info
        {
            get; set;
        }

        public string ItemFile
        {
            get; set;
        }

        public string ItemExportName
        {
            get; set;
        }

        public string GameType
        {
            get; set;
        }

        public string PlayerType
        {
            get; set;
        }

        public string Type
        {
            get; set;
        }

        public int Count
        {
            get; set;
        }

        public int Weight
        {
            get; set;
        }

        public string Spell
        {
            get; set;
        }

        public string MinArena
        {
            get; set;
        }

        public string MaxArena
        {
            get; set;
        }

    }
}