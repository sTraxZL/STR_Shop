using STR_Shop.Models.Data;
using STR_Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}