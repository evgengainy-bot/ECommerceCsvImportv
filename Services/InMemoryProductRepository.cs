using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Хранение товаров в памяти приложения
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new List<Product>();

        public void Add(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            _products.Add(product);
        }

        public IReadOnlyList<Product> GetAll()
        {
            return _products;
        }

        public Product? GetBySku(string sku)
        {
            return _products.FirstOrDefault(p => p.Sku == sku);
        }
    }
}
