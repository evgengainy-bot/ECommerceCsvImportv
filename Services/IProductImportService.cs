using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Интерфейс сервиса импорта товаров из CSV
    public interface IProductImportService
    {
        ImportResult ImportFromCsv(string filePath);
    }
}
