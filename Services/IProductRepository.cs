using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Интерфейс репозитория товаров
    public interface IProductRepository
    {
        void Add(Product product);
        IReadOnlyList<Product> GetAll();
        Product? GetBySku(string sku);
    }
}
