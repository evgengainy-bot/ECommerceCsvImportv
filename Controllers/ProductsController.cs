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

        public ProductsController(IProductRepository repository, IProductImportService importService)
        {
            _repository = repository;
            _importService = importService;
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
                ViewBag.Result = _importService.ImportFromCsv(tempPath);
                System.IO.File.Delete(tempPath);
                return View();
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                ViewBag.Error = "Укажите путь к CSV-файлу";
                return View();
            }

            ViewBag.Result = _importService.ImportFromCsv(filePath);
            return View();
        }
    }
}
