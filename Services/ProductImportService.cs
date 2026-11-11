using System.Globalization;
using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Сервис импорта товаров из CSV-файла с валидацией данных
    public class ProductImportService : IProductImportService
    {
        // границы срока гарантии в месяцах (вариант "Электроника")
        private const int MinWarrantyMonths = 0;
        private const int MaxWarrantyMonths = 60;

        private readonly IProductRepository _repository;

        public ProductImportService(IProductRepository repository)
        {
            _repository = repository;
        }

        // читает CSV-файл, проверяет строки и добавляет товары в репозиторий
        public ImportResult ImportFromCsv(string filePath)
        {
            var result = new ImportResult();

            if (!File.Exists(filePath))
            {
                AddError(result, 0, "Файл не найден");
                return result;
            }

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                AddError(result, 0, "Файл пуст");
                return result;
            }

            if (!lines[0].StartsWith("sku"))
            {
                AddError(result, 1, "Первая строка должна содержать заголовки");
                return result;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                result.TotalRows++;
                int rowNumber = i + 1;

                string[] parts = lines[i].Split(',');
                if (parts.Length < 12)
                {
                    AddError(result, rowNumber, "Неверное количество столбцов");
                    continue;
                }

                string sku = parts[0].Trim();
                string name = parts[1].Trim();
                string description = parts[2].Trim();
                string priceText = parts[3].Trim();
                string currency = parts[4].Trim();
                string category = parts[5].Trim();
                string stockText = parts[6].Trim();
                string imageUrl = parts[7].Trim();
                string activeText = parts[8].Trim().ToLower();
                string brand = parts[9].Trim();
                string warrantyText = parts[10].Trim();
                string modelNumber = parts[11].Trim();

                // обязательные текстовые поля
                if (string.IsNullOrEmpty(sku))
                {
                    AddError(result, rowNumber, "Поле sku не может быть пустым");
                    continue;
                }
                if (string.IsNullOrEmpty(name))
                {
                    AddError(result, rowNumber, "Поле name не может быть пустым");
                    continue;
                }
                if (string.IsNullOrEmpty(brand))
                {
                    AddError(result, rowNumber, "Поле brand обязательно для электроники");
                    continue;
                }

                // цена должна быть числом больше нуля
                if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
                {
                    AddError(result, rowNumber, "Неверный формат цены");
                    continue;
                }
                if (price <= 0)
                {
                    AddError(result, rowNumber, "Цена должна быть больше нуля");
                    continue;
                }

                // остаток - целое число, не меньше нуля
                if (!int.TryParse(stockText, out int stock))
                {
                    AddError(result, rowNumber, "Неверный формат остатка на складе");
                    continue;
                }
                if (stock < 0)
                {
                    AddError(result, rowNumber, "Остаток на складе не может быть отрицательным");
                    continue;
                }

                // признак активности: true/false/1/0
                bool isActive;
                if (activeText == "true" || activeText == "1")
                {
                    isActive = true;
                }
                else if (activeText == "false" || activeText == "0")
                {
                    isActive = false;
                }
                else
                {
                    AddError(result, rowNumber, "Недопустимое значение is_active");
                    continue;
                }

                // срок гарантии от 0 до 60 месяцев
                if (!int.TryParse(warrantyText, out int warranty))
                {
                    AddError(result, rowNumber, "Неверный формат поля warranty_months");
                    continue;
                }
                if (warranty < MinWarrantyMonths || warranty > MaxWarrantyMonths)
                {
                    AddError(result, rowNumber, "Срок гарантии должен быть от " + MinWarrantyMonths + " до " + MaxWarrantyMonths + " месяцев");
                    continue;
                }

                // дубликат артикула пропускаем
                if (_repository.GetBySku(sku) != null)
                {
                    AddError(result, rowNumber, "Артикул товара уже существует");
                    continue;
                }

                var product = new Product();
                product.Sku = sku;
                product.Name = name;
                product.Description = description;
                product.Price = price;
                product.Currency = string.IsNullOrEmpty(currency) ? "RUB" : currency;
                product.CategoryName = string.IsNullOrEmpty(category) ? "Без категории" : category;
                product.StockQuantity = stock;
                product.ImageUrl = imageUrl;
                product.IsActive = isActive;
                product.Brand = brand;
                product.WarrantyMonths = warranty;
                product.ModelNumber = modelNumber;

                _repository.Add(product);
                result.ImportedCount++;
            }

            return result;
        }

        // добавляет ошибку в результат импорта
        private void AddError(ImportResult result, int rowNumber, string message)
        {
            result.Errors.Add(new ImportError { RowNumber = rowNumber, Message = message });
        }
    }
}
