using PagedList;
using STR_Shop.Models.Data;
using STR_Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace STR_Shop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Объявить список категорий CategoryVM
            List<CategoryVM> CategoryList;

            //Инициализировать список данными из базы
            using (Db db = new Db())
            {
                CategoryList = db.Categories
                    .ToArray()
                    .OrderBy(p => p.Sorting)
                    .Select(p => new CategoryVM(p))
                    .ToList();
            }

            //Вернуть список в представление
            return View(CategoryList);
        }

        //Метод сортировки
        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Реализовать начальный счётчик count
                int count = 0;
                //Инициализировать модель
                CategoryDTO dto;
                //Установить сортировку для каждой категории
                foreach (var categoryId in id)
                {
                    dto = db.Categories.Find(categoryId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }

        }

        //Добавление новой категории
        // POST: Admin/Shop/AddNewCategory
        [AcceptVerbs("Get", "Post")]
        public string AddNewCategory(string catName)
        {
            //Объявить строковую переменную id
            string id;
            using (Db db = new Db())
            {
                //Проверить имя категории на уникальность
                if (db.Categories.Any(p => p.Name == catName))
                    return "titletaken";

                //Инициализировать модель DTO
                CategoryDTO dto = new CategoryDTO();

                //Заполнить модель данными
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Сохранить информацию в БД
                db.Categories.Add(dto);
                db.SaveChanges();

                //Получить id, чтобы вернуть его в представление
                id = dto.Id.ToString();
            }

            //Вернуть id в представление
            return id;
        }

        //Удаление категории
        // GET: admin/shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {

                //Получить страницу
                CategoryDTO dto = db.Categories.Find(id);

                //Удалить категорию
                db.Categories.Remove(dto);

                //Сохранить изменения в БД
                db.SaveChanges();
            }

            //Добавить сообщение об успешном удалении страницы
            TempData["SM"] = "You have deleted a category";

            //Переадресовать пользователя
            return RedirectToAction("Categories");
        }

        //Переименование Категории
        // POST: admin/shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //Проверить имя на уникальность
                if(db.Categories.Any(p => p.Name == newCatName))
                {
                    return "titletaken";
                }

                //Получить модель DTO
                CategoryDTO dto = db.Categories.Find(id);

                //Отредактировать DTO с новыми значениями
                dto.Name = newCatName;
                dto.Slug.Replace(" ", "-").ToLower();

                //Сохранить изменения в базе
                db.SaveChanges();
            }
            //Вернуть слово
            return "ok";
        }

        //Добавление товаров
        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Объявить модель DTO
            ProductVM model = new ProductVM();
            using (Db db = new Db())
            {
                //Добавить список категорий в модель
                model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "id", dataTextField: "Name");

            }
            //Вернуть модель в представление
            return View(model);
        }

        //Добавление товаров
        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Проверить модель на валидность

            if(!ModelState.IsValid)
            {
                using(Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // Проверить имя продукта на уникальность

            using(Db db = new Db())
            {
                if(db.Products.Any(p => p.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "This product name is taken!");
                    return View(model);
                }
            }

            // Объявить переменную ProductID

            int id;

            // Инициализировать и сохранить модель на основе ProductDTO в базу

            using (Db db = new Db())
            {

                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ","-").ToLower();
                product.Discription = model.Discription;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(p => p.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            // Вывести сообщение TempData пользователю

            TempData["SM"] = "You have added a product";

            #region Upload image

            // создать необходимые ссылки директории для файлов

            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}.Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            // Проверить, имеется ли дирректория по заданным ссылкам (если нет - создать)

            if(!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }

            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }

            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }

            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }

            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            // Проверить, был ли файл загружен

            if (file != null && file.ContentLength > 0)
            {
                //Получить расширение файла

                string ext = file.ContentType.ToLower();

                // Проверить расширение файла

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    )
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Image does not uploaded - wrong image extansion");
                        return View(model);
                    }
                }




                // Объявить переменную с именем файла

                string imageName = file.FileName;

                // Сохранить файл в модель DTO

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // Назначить 2 пути, к оригиналу и уменьшинному варианту изображения

                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                // Сохранить оригинальное изображение
                file.SaveAs(path);

                // Создать и сохранить уменьшенное изображение
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }
            #endregion

            //Переадресовать пользователя

            return RedirectToAction("AddProduct");
        }

        //Список товаров
        // GET: Admin/Shop/Products/id
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Объявить список ProductVM list
            List<ProductVM> listOfProductVM;

            //Установить номер страницы
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Инициализировать и заполнить list данными
                listOfProductVM = db.Products.ToArray()
                    .Where(p => catId == null || catId == 0 || p.CategoryId == catId)
                    .Select(p => new ProductVM(p))
                    .ToList();

                //Заполнить категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Установить выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }
            //Установить постраничную навигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            //Вернуть представление с данными
            return View(listOfProductVM);
        }

        //Редактирование товаров
        // GET: Admin/Shop/EditProduct
        public ActionResult EditProduct(int? id)
        {
            //Объявить модель ProductVM
            ProductVM product = new ProductVM();

            using (Db db = new Db())
            {
                //Получить продукт
                ProductDTO dto = db.Products.Find(id);

                //Проверить валидность продукта
                if (dto == null)
                {
                    return Content("That product is not exist");
                }
                else
                {
                    //Инициализировать модель данными
                    product = new ProductVM(dto);

                    //Создать список категорий (list)
                    product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                    //Получить все изображения из галереи
                    product.GalleryImages = Directory
                        .EnumerateFiles(Server.MapPath("~/.Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                        .Select(fn => Path.GetFileName(fn));
                }
            }
            //Вернуть модель в представление
            return View(product);
        }


        //Редактирование товаров
        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Получить id продукта
            int id = model.Id;

            using (Db db = new Db())
            {
                //Заполнить список продуктами и изображениями
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory
                        .EnumerateFiles(Server.MapPath("~/.Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                        .Select(fn => Path.GetFileName(fn));

            //Проверить модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //Проверить имя продукта на уникальность
                if (db.Products.Where(p => p.Id != id).Any(p => p.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
            }
            //Внести изменения в базу
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Discription = model.Discription;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(p => p.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            //Установить сообщение в TempData
            TempData["SM"] = "You have edited a product";

            //Реализовать обработку изображений 
            #region Image upload

            //Проверить загрузку файла
            if (file != null && file.ContentLength > 0)
            {

                //Получить расширение файла
                string ext = file.ContentType.ToLower();

                //Проверить расширение
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    )
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Image does not uploaded - wrong image extansion");
                        return View(model);
                    }
                }

                //Установить пути загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}.Images\\Uploads"));
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Удалить файлы, если они уже присутствуют в директориях и сами директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach(var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //Сохранить имя изображения
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    //Инициализировать модель
                    ProductDTO dto = db.Products.Find(id);

                    //Присвоить имя картинки
                    dto.ImageName = imageName;

                    //Сохранить изменения
                    db.SaveChanges();
                }
                // Назначить 2 пути, к оригиналу и уменьшинному варианту изображения

                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                // Сохранить оригинальное изображение
                file.SaveAs(path);

                // Создать и сохранить уменьшенное изображение
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            //Переадресовать пользователя
            return RedirectToAction("EditProduct");
        }

        //Удаление товара
        // GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            using (Db db = new Db())
            {
                //Инициализировать DTO
                ProductDTO dto = db.Products.Find(id);

                //Удалить товар из базы данных
                db.Products.Remove(dto);

                //Сохранить изменения
                db.SaveChanges();
            }

            //Удалить изображения, товара и дитректории
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}.Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            //Переадресовать пользователя
            return RedirectToAction("Products");
        }

    }
}