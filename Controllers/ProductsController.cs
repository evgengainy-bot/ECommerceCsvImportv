using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Controllers
{
    // Контроллер страниц импорта и списка товаров
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IProductImportService _importService;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductRepository repository, IProductImportService importService, IWebHostEnvironment env)
        {
            _repository = repository;
            _importService = importService;
            _env = env;
        }

        // список товаров из памяти
        public ActionResult Index()
        {
            var products = _repository.GetAll();
            return View(products);
        }

        // форма импорта
        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }

        // приводит путь к файлу к абсолютному: убирает кавычки и лишние пробелы,
        // ищет файл относительно папки проекта и wwwroot
        private string ResolvePath(string filePath)
        {
            string path = filePath.Trim().Trim('"').Trim();

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            string candidate = Path.Combine(_env.ContentRootPath, path);
            if (System.IO.File.Exists(candidate))
            {
                return candidate;
            }

            if (path.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
            {
                string relative = path.Substring(7).TrimStart('\\', '/');
                candidate = Path.Combine(_env.WebRootPath, relative);
                if (System.IO.File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return path;
        }

        // возвращает ImportResult с единственной ошибкой вместо исключения
        private ImportResult ErrorResult(string message)
        {
            var result = new ImportResult();
            result.Errors.Add(new ImportError { RowNumber = 0, Message = message });
            return result;
        }

        // запуск импорта: файл из браузера или путь к файлу на сервере
        [HttpPost]
        public ActionResult Import(IFormFile file, string filePath)
        {
            if (file != null && file.Length > 0)
            {
                string tempPath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(tempPath))
                {
                    file.CopyTo(stream);
                }

                try
                {
                    ViewBag.Result = _importService.ImportFromCsv(tempPath);
                }
                catch (Exception ex)
                {
                    ViewBag.Result = ErrorResult("Непредвиденная ошибка: " + ex.Message);
                }

                System.IO.File.Delete(tempPath);
                return View();
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                ViewBag.Error = "Укажите путь к CSV-файлу";
                return View();
            }

            try
            {
                ViewBag.Result = _importService.ImportFromCsv(ResolvePath(filePath));
            }
            catch (Exception ex)
            {
                ViewBag.Result = ErrorResult("Непредвиденная ошибка: " + ex.Message);
            }

            return View();
        }
    }
}
