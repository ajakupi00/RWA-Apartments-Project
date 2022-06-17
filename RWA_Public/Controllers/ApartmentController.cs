﻿using Newtonsoft.Json;
using RWA_Library.DAL;
using RWA_Library.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Converters;
using System.Text;

namespace RWA_Public.Controllers
{
    public class ApartmentController : Controller
    {
        private IRepo _repo;
        private Apartment[] apt;
        private List<string> cookies = new List<string>(){"city", "sort", "rooms", "children", "adults"};
        public ApartmentController()
        {
            _repo = RepoFactory.GetRepo();
            apt = _repo.GetAllApartments().ToArray();
        }


        [HttpGet]
        public ActionResult Index(string search)
        {
            if (Request.Cookies.Count < 6)
            {
                Response.Cookies.Add(new HttpCookie("city", ""));
                Response.Cookies.Add(new HttpCookie("sort", ""));
                Response.Cookies.Add(new HttpCookie("rooms", "0"));
                Response.Cookies.Add(new HttpCookie("children", "0"));
                Response.Cookies.Add(new HttpCookie("adults", "0"));
                Response.Cookies.Add(new HttpCookie("language", "eng"));
            }
           

            ViewBag.user = Session["user"];
            ViewBag.cities = _repo.GetAllCities();

            if (search is null) search = "";

            string language = HttpContext.Request.Cookies["language"].Value;
            ViewBag.lang = language;
            Response.Cookies.Add(new HttpCookie("search", search));
            return View("Index");
        }

        [HttpPost]
        public ActionResult Filter(string rooms, string children, string adults, string city, string sort)
        {
            ViewBag.user = Session["user"];
            ViewBag.repo = _repo;
            ViewBag.cities = _repo.GetAllCities();
            string language = HttpContext.Request.Cookies["language"].Value;
            ViewBag.lang = language;
            Request.Cookies["city"].Value = city;
            Request.Cookies["children"].Value = children;
            Request.Cookies["adults"].Value = adults;
            Request.Cookies["rooms"].Value = rooms;
            Response.Cookies["sort"].Value = sort;

            return View("Index");
        }

        [HttpGet]
        public ActionResult Reserve(int id)
        {

            ViewBag.apt = _repo.GetApartmentByID(id);
            if (ViewBag.apt is null)
            {
                throw new Exception("Apartment with that ID does not exists!");
            }

            string language = HttpContext.Request.Cookies["language"].Value;
            ViewBag.lang = language;
            ViewBag.user = Session["user"];
            ViewBag.pictures = _repo.GetPicturesByApartmentID(id);
            ViewBag.tags = _repo.GetTagsByApartmentID(id);
            return View();
        }


        public JsonResult GetApartments()
        {
            string search = Request.Cookies["search"].Value;
            var data = apt.ToArray();
            foreach (Apartment apartment in data)
            {
                apartment.Representative = _repo.GetPicturesByApartmentID(apartment.Id).FirstOrDefault(p => p.IsRepresentative).Path;
            }
            if (HttpContext.Request.Cookies["language"].Value == "hr")
                data = data.Where(a => a.Name.ToLower().Contains(search.ToLower())).ToArray();
            else
                data = data.Where(a => a.NameEng.ToLower().Contains(search.ToLower())).ToArray();

            string sort = HttpContext.Request.Cookies["sort"].Value;
            switch (sort)
            {
                case "pu":
                    Array.Sort(data, (x,y) => x.Price.CompareTo(y.Price));
                    break;
                case "ps":
                    Array.Sort(data, (x, y) => -x.Price.CompareTo(y.Price));
                    break;
                default:
                    Array.Sort(data);
                    break;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRPictureByApartment(int id)
        {
            ApartmentPicture picture = (ApartmentPicture)_repo.GetPicturesByApartmentID(id).FirstOrDefault(p => p.IsRepresentative);
            return Json(picture, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPicturesByApartment(int id)
        {
            IList<ApartmentPicture> pictures = _repo.GetPicturesByApartmentID(id).Where(p => !p.IsRepresentative).ToList();
            return Json(pictures, JsonRequestBehavior.AllowGet);
        }
    }
}