using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using STR_Shop.Models.Data;
using System.Web.Mvc;

namespace STR_Shop.Models.ViewModels.Pages
{
    public class PageVM
    {
        public PageVM()
        {

        }

        public PageVM(PagesDTO row)
            {
            Id = row.Id;
            Title = row.Title;
            Slug = row.Slug;
            Body = row.Body;
            Sorting = row.Sorting;
            HasSideBar = row.HasSideBar;
            }


        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength =3)]
        public string Title { get; set; }
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength =3)]
        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }
        [Display(Name ="SideBar")]
        public bool HasSideBar { get; set; }
    }
}