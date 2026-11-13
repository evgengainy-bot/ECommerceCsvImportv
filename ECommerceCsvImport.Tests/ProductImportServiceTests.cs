using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    // Тесты сервиса импорта ProductImportService по сценариям задания
    public class ProductImportServiceTests
    {
        private const string Header =
            "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number";

        private const string ValidRow =
            "2001,Тест Товар,Описание товара,1000,RUB,Категория,5,/img/test.jpg,true,ТестБренд,12,TM-1";

        private readonly IProductRepository _repository = new InMemoryProductRepository();
        private readonly IProductImportService _service;

        public ProductImportServiceTests()
        {
            _service = new ProductImportService(_repository);
        }

        private string WriteCsv(params string[] lines)
        {
            string path = System.IO.Path.GetTempFileName() + ".csv";
            System.IO.File.WriteAllLines(path, lines);
            return path;
        }

        private ImportResult Import(string path)
        {
            return _service.ImportFromCsv(path);
        }

        [Fact]
        public void ImportFromCsv_ValidRows_ImportsAllRows()
        {
            string path = WriteCsv(Header, ValidRow, ValidRow.Replace("2001", "2002"));
            var result = Import(path);
            Assert.Equal(2, result.TotalRows);
            Assert.Equal(2, result.ImportedCount);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ImportFromCsv_MissingFile_AddsFileNotFound()
        {
            var result = Import("C:\\no_such_file_12345.csv");
            Assert.Equal(0, result.ImportedCount);
            Assert.Contains("Файл не найден", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_EmptyFile_AddsFileIsEmpty()
        {
            string path = WriteCsv();
            var result = Import(path);
            Assert.Contains("Файл пуст", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_WrongHeader_AddsHeaderError()
        {
            string path = WriteCsv("name,price", "Тест,100");
            var result = Import(path);
            Assert.Contains("Первая строка должна содержать заголовки", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_TooFewColumns_AddsColumnError()
        {
            string path = WriteCsv(Header, "2001,Тест Товар");
            var result = Import(path);
            Assert.Equal(1, result.TotalRows);
            Assert.Equal(0, result.ImportedCount);
            Assert.Contains("Неверное количество столбцов", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_NegativePrice_AddsPriceError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",1000,", ",-500,"));
            var result = Import(path);
            Assert.Equal(0, result.ImportedCount);
            Assert.Contains("Цена должна быть больше нуля", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_NonNumericPrice_AddsPriceFormatError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",1000,", ",abc,"));
            var result = Import(path);
            Assert.Contains("Неверный формат цены", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_NegativeStock_AddsStockError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",5,", ",-2,"));
            var result = Import(path);
            Assert.Contains("Остаток на складе не может быть отрицательным", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_NonNumericStock_AddsStockFormatError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",5,", ",много,"));
            var result = Import(path);
            Assert.Contains("Неверный формат остатка на складе", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_BadIsActive_AddsActiveError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",true,", ",да,"));
            var result = Import(path);
            Assert.Contains("Недопустимое значение is_active", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_EmptyBrand_AddsBrandError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",ТестБренд,", ",,"));
            var result = Import(path);
            Assert.Contains("Поле brand обязательно для электроники", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_EmptySku_AddsSkuError()
        {
            string path = WriteCsv(Header, ValidRow.Replace("2001,", ","));
            var result = Import(path);
            Assert.Contains("Поле sku не может быть пустым", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_EmptyName_AddsNameError()
        {
            string path = WriteCsv(Header, ValidRow.Replace("Тест Товар,", ","));
            var result = Import(path);
            Assert.Contains("Поле name не может быть пустым", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_WarrantyOutOfRange_AddsWarrantyError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",12,TM-1", ",120,TM-1"));
            var result = Import(path);
            Assert.Contains("Срок гарантии должен быть от 0 до 60 месяцев", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_WarrantyNotNumber_AddsWarrantyFormatError()
        {
            string path = WriteCsv(Header, ValidRow.Replace(",12,TM-1", ",два,TM-1"));
            var result = Import(path);
            Assert.Contains("Неверный формат поля warranty_months", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_DuplicateSkuInFile_SecondsRowIsSkipped()
        {
            string path = WriteCsv(Header, ValidRow, ValidRow);
            var result = Import(path);
            Assert.Equal(2, result.TotalRows);
            Assert.Equal(1, result.ImportedCount);
            Assert.Contains("Артикул товара уже существует", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_DuplicateSkuInMemory_SecondsRowIsSkipped()
        {
            _repository.Add(new Product { Sku = "2001", Name = "Старый товар" });
            string path = WriteCsv(Header, ValidRow);
            var result = Import(path);
            Assert.Equal(0, result.ImportedCount);
            Assert.Contains("Артикул товара уже существует", result.Errors.Select(e => e.Message));
        }

        [Fact]
        public void ImportFromCsv_EmptyCurrencyAndCategory_UsesDefaults()
        {
            string path = WriteCsv(Header, "2001,Тест Товар,,1000,,,5,/img/test.jpg,true,ТестБренд,12,TM-1");
            var result = Import(path);
            Assert.Equal(1, result.ImportedCount);
            var product = Assert.Single(_repository.GetAll());
            Assert.Equal("RUB", product.Currency);
            Assert.Equal("Без категории", product.CategoryName);
        }

        [Fact]
        public void ImportFromCsv_EmptyLinesAreSkipped()
        {
            string path = WriteCsv(Header, ValidRow, "", "", ValidRow.Replace("2001", "2002"));
            var result = Import(path);
            Assert.Equal(2, result.TotalRows);
            Assert.Equal(2, result.ImportedCount);
        }
    }
}
