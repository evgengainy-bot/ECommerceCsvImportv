namespace ECommerceCsvImport.Models
{
    // Результат импорта товаров из CSV
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();

        // количество ошибок в результате импорта
        public int ErrorCount
        {
            get { return Errors.Count; }
        }
    }
}
