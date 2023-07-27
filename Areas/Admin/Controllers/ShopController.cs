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

                var path = string.Format($"{pathString2}\\imageName");
                var path2 = string.Format($"{pathString3}\\imageName");

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

    }
}