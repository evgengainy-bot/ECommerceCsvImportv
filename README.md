# ECommerceCsvImport

Учебный проект по производственной практике ПП.02.01 «Технология разработки программного обеспечения» (вариант 1 — Электроника).

Модуль импорта товаров из CSV-файлов в каталог интернет-магазина на ASP.NET MVC.

## Функциональность

- чтение CSV-файла с заголовками (кодировка UTF-8);
- валидация строк: sku и name обязательны, price > 0, stock >= 0, is_active = true/false/1/0, brand обязательно (электроника), warranty_months от 0 до 60;
- дубликаты артикулов пропускаются с ошибкой;
- хранение товаров в памяти (InMemoryProductRepository);
- страница импорта /Products/Import (загрузка файла через браузер или путь на сервере);
- страница списка товаров /Products/Index.

## Формат CSV

sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number

## Тестовые файлы (wwwroot/csv)

- sample_products.csv — пример каталога;
- valid_electronics.csv — корректный файл (успешный импорт);
- invalid_price.csv — отрицательная цена;
- duplicate_sku.csv — дубликат артикула;
- missing_brand.csv — пустой бренд;
- warranty_out_of_range.csv — гарантия 120 месяцев;
- bad_active.csv — недопустимое значение is_active;
- empty_file.csv — пустой файл.

## Запуск

Visual Studio 2022, .NET 8. Открыть проект, запустить (F5), в меню выбрать «Товары».
