using STR_Shop.Models.Data;
using STR_Shop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace STR_Shop.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Объявить список страниц для представления (PageVM)
            List<PageVM> PageList;
            //Заполнить список информацией из базы данных
            using (Db db = new Db())
            {
                PageList = db.Pages
                    .ToArray()
                    .OrderBy(p => p.Sorting)
                    .Select(p => new PageVM(p))
                    .ToList();
            }
            //Вернуть список в представление
            return View(PageList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Проверка модели на валидность
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db= new Db())
            {
                //Объявить отдельную переменную для краткого описания (slug)
                string slug;

                //Инициализировать класс PageDTO
                PagesDTO dto = new PagesDTO();

                //Присвоить заголовок моделе
                dto.Title = model.Title.ToUpper();

                //Проверка, имеется ли slug, если нет, присвоить его
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                    slug = model.Slug.Replace(" ", "-").ToLower();

                //Проверка заголовка и slug на уникальность
                if(db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title allready exist.");
                    return View(model);
                }
                else if(db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug allready exist.");
                    return View(model);
                }

                //Присвоить оставшиеся значения модели
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                dto.Sorting = 100;

                //Сохранить модель в базу данных
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //Выдать сообщение пользователю через TempData
            TempData["SM"] = "You have added a new page";

            //Переадресация пользователя на Index
            return RedirectToAction("Index");
        }

        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Объявить модель PageVM
            PageVM model;
            using (Db db = new Db())
            {
                //Получить страницу по id
                PagesDTO dto = db.Pages.Find(id);
                //Проверить, доступна ли страница (существует ли она)
                if(dto == null)
                {
                    return Content("this page does not exist");
                }
                //Инициализировать модель данными из PageDTO
                model = new PageVM(dto);
            }
            //Вернуть модель в представление
            return View(model);
        }

        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Проверить модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            using (Db db = new Db())
            {
                //Получить id страницы
                int id = model.Id;

                //Объявить переменную для slug
                string slug = "home";

                //Получить страницу по id
                PagesDTO dto = db.Pages.Find(id);

                //Присвоить в DTO новые полученные свойства
                dto.Title = model.Title;

                //Проверить, есть ли slug, если нет, то присвоить
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Проверить slug и title на уникальность
                if(db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist");
                    return View(model);
                }
                else if(db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug already exist");
                    return View(model);
                }

                //Присвоить значения в DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;

                //Сохранить изменения в базе данных
                db.SaveChanges();
            }
            //Вывести оповещение пользователю в TempData
            TempData["SM"] = "You have edited this page";

            //Переадресовать пользователя на страницу, которую он редактировал
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails/id
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            //Объявить модель PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Получить страницу из базы данных
                PagesDTO dto = db.Pages.Find(id);
                //Убедиться, что страница доступна (существует)
                if (dto == null)
                {
                    return Content("this page does not exist");
                }
                //Присвоить модели поля из базы данных
                model = new PageVM(dto);

            }
            //Вернуть модель в представление
            return View(model);
        }

        //Удаление страницы
        // GET: Admin/Pages/DeletePage/id
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {

                //Получить страницу
                PagesDTO dto = db.Pages.Find(id);

                //Удалить страницу
                db.Pages.Remove(dto);

                //Сохранить изменения в БД
                db.SaveChanges();
            }

            //Добавить сообщение об успешном удалении страницы
            TempData["SM"] = "You have deleted a page";

            //Переадресовать пользователя
            return RedirectToAction("Index");
        }

        //Метод сортировки
        // POST: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int [] id)
        {
            using (Db db = new Db())
            {
                //Реализовать начальный счётчик count
                int count = 0;
                //Инициализировать модель
                PagesDTO dto;
                //Установить сортировку для каждой страницы
                foreach(var PageId in id)
                {
                    dto = db.Pages.Find(PageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }

        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Объявить модель
            SidebarVM model;

            using (Db db = new Db())
            {
                SidebarDTO dto;
                //Получить данные из DTO
                dto = db.Sidebars.Find(1); // тестовый говнокод - исправить после теста
                //Заполнить модель
                model = new SidebarVM(dto);
            }
            //Вернуть представление с моделью
            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //Получить данные из DTO
                SidebarDTO dto = db.Sidebars.Find(1); // говнокод - исправить после теста
                //Присвоить данные в body
                dto.Body = model.Body;
                //Проверить, что body не NULL
                if(dto.Body != null)
                {
                    db.SaveChanges();
                }
                else
                {
                    TempData["SM"] = "Body is null";
                    return View(model);
                }
                //Обновить информацию в БД
            }
            //Вывести сообщение об удачном обновлении в TempData
            TempData["SM"] = "You have edited the Sidebar";
            //Переадресовать пользователя
            return RedirectToAction("EditSidebar");
        }

    }


}