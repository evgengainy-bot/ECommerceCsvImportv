using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    // Тесты хранилища товаров в памяти
    public class RepositoryTests
    {
        private Product TestProduct()
        {
            return new Product { Sku = "1001", Name = "Тест товар", Price = 1000 };
        }

        [Fact]
        public void GetAll_ReturnsEmptyForNewRepository()
        {
            var repo = new InMemoryProductRepository();
            Assert.Empty(repo.GetAll());
        }

        [Fact]
        public void Add_AllowsGetAllToReturnProduct()
        {
            var repo = new InMemoryProductRepository();
            repo.Add(TestProduct());
            Assert.Single(repo.GetAll());
        }

        [Fact]
        public void GetBySku_ReturnsProductBySku()
        {
            var repo = new InMemoryProductRepository();
            repo.Add(TestProduct());
            var found = repo.GetBySku("1001");
            Assert.NotNull(found);
            Assert.Equal("Тест товар", found!.Name);
        }

        [Fact]
        public void GetBySku_ReturnsNullWhenMissing()
        {
            var repo = new InMemoryProductRepository();
            repo.Add(TestProduct());
            Assert.Null(repo.GetBySku("9999"));
        }
    }
}
