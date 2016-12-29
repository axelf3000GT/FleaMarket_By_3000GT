using System;
using System.Collections.Generic;
using System.Linq;
using FleaMarket.Models;
using System.Web.UI.WebControls;
using System.Data.Entity;
using System.Web.Mvc;
using System.IO;
using System.Web; 
using PagedList.Mvc;
using PagedList;

namespace FleaMarket.Controllers
{
    public class FleaMarketController : Controller
    {
        const int pageSize = 8;
        FleaContext db = new FleaContext();
        public bool isNewlyEnrolled { get; set; }
        public void InitDropDownList()
        {
            //для DropDownList добавление значений
            IEnumerable<AdCat> adCat = db.AdCats;
            SelectList AdCats = new SelectList(adCat, "AdCatId", "AdCatSubHeader");
            ViewBag.AdCat = AdCats;
            //------------------------------
            IEnumerable<AdLocation> adLocation = db.AdLocations;
            SelectList AdLocations = new SelectList(adLocation, "AdLocationId", "AdLocationArea");
            ViewBag.AdLocation = AdLocations; 
            //------------------------------
            IEnumerable<AdType> adType = db.AdTypes;
            SelectList AdTypes = new SelectList(adType, "AdTypeId", "AdTypeName");
            ViewBag.AdType = AdTypes;
            //------------------------------
            var query = (from m in db.AdLocations
                     select m.AdLocationCity)
                        .Distinct();
            SelectList AdLocationCitys = new SelectList(query);
            //------------------------------

            ViewBag.AdLocationCity = AdLocationCitys;
            ViewBag.AdSortDate = new SelectList(new List<SelectListItem>{
                                 new SelectListItem {Selected = true, Text = "новые", Value = "-1"},
                                 new SelectListItem {Selected = false, Text = "старые", Value = "2"},
                                 }, "Value", "Text");

            ViewBag.AdSortPrice = new SelectList(new List<SelectListItem>{
                                 new SelectListItem {Selected = true, Text = "не имеет значения", Value = "-1"},
                                 new SelectListItem {Selected = false, Text = "дешевые", Value = "2"},
                                 new SelectListItem {Selected = false, Text = "дорогие", Value = "3"},
                                 }, "Value", "Text"); 
        }
        public IQueryable<FleaMarket.Models.Ad>  Filter(int? AdCatId, int? AdSortPriceId, int? AdSortDateId, int? AdTypeId, string searchAdHeader)
        {
            //Объединение таблиц |
            var list = db.Ads
                .Include(p => p.AdCat)
                .Include(p => p.AdLocation)
                .Include(p => p.AdType)
                .Where(p => p.AdTypeId == AdTypeId)
                .Where(p => p.AdIsEnable == true)
                .Where(p => p.AdHeader.Contains(searchAdHeader));
            if (AdCatId != 1)
                list = list
                        .Where(p => p.AdCatId == AdCatId)
                        .Where(p => p.AdTypeId == AdTypeId);

            if (AdSortDateId == 2 && AdSortPriceId == -1)
            {
                list = list
                    .OrderBy(p => p.AdDateUpdate);
            }
            else if (AdSortDateId == -1 && AdSortPriceId == 3)
            {
                list = list
                    .OrderByDescending(p => p.AdDateUpdate)
                    .OrderByDescending(p => p.AdCost);
            }
            else if (AdSortDateId == -1 && AdSortPriceId == 2)
            {
                list = list
                    .OrderByDescending(p => p.AdDateUpdate)
                    .OrderBy(p => p.AdCost);
            }
            else if (AdSortDateId == 2 && AdSortPriceId == 2)
            {
                list = list
                    .OrderByDescending(p => p.AdDateUpdate)
                    .OrderBy(p => p.AdCost);
            }
            else if (AdSortDateId == 2 && AdSortPriceId == 3)
            {
                list = list
                    .OrderByDescending(p => p.AdDateUpdate)
                    .OrderByDescending(p => p.AdCost);
            }
            else
                list = list
                    .OrderByDescending(p => p.AdDateUpdate);
            return (list);
        }

        //РАБОТА С ОБЪЯВЛЕНИЯМИ
        [HttpGet]
        [RequireHttps]
        public ActionResult Index(int? page)
        { 
            InitDropDownList();
            //Объединение таблиц |
            var list = db.Ads
                .Include(p => p.AdCat)
                .Include(p => p.AdLocation)
                .Include(p => p.AdType)
                .Where(p => p.AdIsEnable == true);
            list = list.OrderByDescending(p => p.AdDateUpdate);
            //------------------------------ 
            int pageSize = 3;
            int pageNumber = (page ?? 1); 
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        //РАБОТА С ОБЪЯВЛЕНИЯМИ
        [HttpPost]
        public ActionResult Index(int? AdCatId, int? AdSortPriceId, int? AdSortDateId, int? AdTypeId, string searchAdHeader, int? page)
        { 
            InitDropDownList(); 
            var list = Filter(AdCatId, AdSortPriceId, AdSortDateId, AdTypeId, searchAdHeader);
            int pageSize = 3;
                int pageNumber = (page ?? 1);
                return PartialView(list.ToPagedList(pageNumber, pageSize));
        }


        //Добавление объявления только для зарегистрированных пользователей
        //SELECT CITY
        [Authorize(Roles = "user")]
        public ActionResult AddNewAdsSelectCity()
        {
            //---------выбор страны--------------------
            IEnumerable<AdLocation> adLocationCountry = db.AdLocations;
            var query = (from m in db.AdLocations
                         select m.AdLocationCountry)
                         .Distinct();
            SelectList AdLocationCountrys = new SelectList(query);
            ViewBag.AdLocationCountry = AdLocationCountrys;
            //---------выбор города--------------------
            query = (from m in db.AdLocations
                     select m.AdLocationCity)
                         .Distinct();
            SelectList AdLocationCitys = new SelectList(query);
            ViewBag.AdLocationCity = AdLocationCitys;
            //---------выбор типа объявления-----------
            IEnumerable<AdType> adType = db.AdTypes;
            SelectList AdTypes = new SelectList(adType, "AdTypeId", "AdTypeName");
            ViewBag.AdType = AdTypes;
            //---------выбор категорий-----------
            query = (from m in db.AdCats
                     where m.AdCatHeader != "Все"
                     select m.AdCatHeader)
                         .Distinct();
            SelectList AdCatHeader = new SelectList(query);
            ViewBag.AdCatHeader = AdCatHeader;
            //------------------------------------------
            return View();
        }
        //REDITERCT TO ADD AD
        [HttpPost]
        public ActionResult AddNewAdsSelectCity(Ad ad)
        {
            string country = Request.Form["AdLocationCountry"].ToString();
            string city = Request.Form["AdLocationCity"].ToString();
            string catHeader = Request.Form["AdCatHeader"].ToString();
            int type = Convert.ToInt16(Request.Form["AdType"]);
            return RedirectToAction("AddNewAds", "FleaMarket", new { city, country, type, catHeader });
        }


        //ADD AD
        //Добавление объявления только для зарегистрированных пользователей
        [Authorize(Roles = "user")]
        public ActionResult AddNewAds(String city, String country, String catHeader, int type)
        {
            //для DropDownList добавление категорий
            var query = (from m in db.AdCats
                         where m.AdCatHeader == catHeader
                         select m.AdCatSubHeader);
            SelectList AdCatSubHeader = new SelectList(query);
            ViewBag.AdCatSubHeader = AdCatSubHeader;
            //--передача параметров принятых-
            ViewBag.AdLocationCountry = country;
            ViewBag.AdLocationCity = city;
            //--по индексу берем описание типа объявления
            var list = db.Ads
                         .Include(p => p.AdCat)
                         .Include(p => p.AdType)
                         .Include(p => p.AdLocation);
            String buf = "";
            var typeName = db.AdTypes.Where(p => p.AdTypeId == type);
            foreach (var item in typeName)
                buf = item.AdTypeName;
            ViewBag.AdType = buf;
            ViewBag.AdTypeId = type;
            //-запрос списка районов для данной категорий----
            query = (from m in db.AdLocations
                     where m.AdLocationCity == city
                     select m.AdLocationArea)
                         .Distinct();
            SelectList AdLocations = new SelectList(query);
            ViewBag.AdLocationArea = AdLocations;
            //------------------------------
            return View();
        }
        //сохранение объявления
        [HttpPost]
        public ActionResult AddNewAds(Ad ad, int type, String city, String country, String catHeader, Boolean IsImages)
        { 
            ad.AdDateAdd = DateTime.Now;
            ad.AdDateLastEditing = DateTime.Now;
            ad.AdDateUpdate = DateTime.Now;
            ad.AdUserId = User.Identity.Name;
            ad.AdTypeId = type;
            ad.AdCountWatchingPhone = 0;
            ad.AdCountWatchngAd = 0;
            ad.AdIsEnable = true; 
            //Получаем номер конечного месторасположения - надо это как то укоротить
            String AdLocationArea = Request.Form["AdLocationArea"];
            var query = (from m in db.AdLocations
                         where m.AdLocationCity == city &&
                               m.AdLocationCountry == country &&
                               m.AdLocationArea == AdLocationArea
                         select m.AdLocationId);
            SelectList AdLocation = new SelectList(query);
            string buf = "";
            foreach (var item in AdLocation)
                buf = item.Text;
            ad.AdLocationId = Convert.ToInt32(buf);
            //Получаем конечный номер - надо это как то укоротить
            String AdCatSubHeader = Request.Form["AdCatSubHeader"];
            query = (from m in db.AdCats
                     where m.AdCatHeader == catHeader &&
                           m.AdCatSubHeader == AdCatSubHeader
                     select m.AdCatId);
            SelectList AdCatId = new SelectList(query);
            buf = "";
            foreach (var item in AdCatId)
                buf = item.Text;
            ad.AdCatId = Convert.ToInt32(buf);
            //---------------------
            db.Ads.Add(ad);// добавляем информацию о покупке в базу данных
            db.SaveChanges();// сохраняем в бд все изменения
            TempData["AdId"] = ad.AdId;

            if (IsImages == true)
                return RedirectToAction("AddNewAdsPictures", "FleaMarket");
            else
                return RedirectToAction("Index", "FleaMarket");
        }
        [HttpGet]
        [Authorize(Roles = "user")]
        public ActionResult AddNewAdsPictures()
        { 
            return View(); 
        }

        [HttpPost]
        public ActionResult AddNewAdsPictures(HttpPostedFileBase uploadImage1, HttpPostedFileBase uploadImage2,
                                              HttpPostedFileBase uploadImage3, HttpPostedFileBase uploadImage4,
                                              HttpPostedFileBase uploadImage5, HttpPostedFileBase uploadImage6)
        {
            Ad ad = db.Ads.Find(TempData["AdId"]);
            if (ad != null)
            {
                if (ModelState.IsValid && uploadImage1 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage1.InputStream))
                    {
                        ad.Image1 = binaryReader.ReadBytes(uploadImage1.ContentLength);
                    }
                }
                if (ModelState.IsValid && uploadImage2 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage2.InputStream))
                    {
                        ad.Image2 = binaryReader.ReadBytes(uploadImage2.ContentLength);
                    }
                }
                if (ModelState.IsValid && uploadImage3 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage3.InputStream))
                    {
                        ad.Image3 = binaryReader.ReadBytes(uploadImage3.ContentLength);
                    }
                }
                if (ModelState.IsValid && uploadImage4 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage4.InputStream))
                    {
                        ad.Image4 = binaryReader.ReadBytes(uploadImage4.ContentLength);
                    }
                }
                if (ModelState.IsValid && uploadImage5 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage5.InputStream))
                    {
                        ad.Image5 = binaryReader.ReadBytes(uploadImage5.ContentLength);
                    }
                }
                if (ModelState.IsValid && uploadImage6 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage6.InputStream))
                    {
                        ad.Image6 = binaryReader.ReadBytes(uploadImage6.ContentLength);
                    }
                }
                db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
                db.SaveChanges();

            }
            return RedirectToAction("Index", "FleaMarket");
        }
        //описание объявления
        [HttpGet]
        public ActionResult DescriptionAds(int? id)
        {
            if (id != null)
            {
                Ad ad = db.Ads.Find(id); //инкрементируем счетчик просмотра объявлений
                if (ad != null)
                    ad.AdCountWatchngAd++;
                db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
                db.SaveChanges();
                var list = db.Ads //объединяем таблички для просмотра
                            .Include(p => p.AdCat)
                            .Include(p => p.AdType)
                            .Include(p => p.AdLocation)
                            .Where(p => p.AdId == id)
                            .First();
                if (list != null)
                    return View(list);
            }
            return HttpNotFound();
        }

        //редактирование объявления
        [Authorize(Roles = "user")]
        [HttpGet]
        public ActionResult EditAds(int? id, int? AdId)
        {
            InitDropDownList();
            //------------------------------
            //Установка позиции на выбранное поле
            if (AdId != null) id = AdId;
            Ad ads = db.Ads.Find(id);
            if (ads != null)
            {
                if (ads.AdUserId == User.Identity.Name)
                {
                    return View(ads);
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        }
        [Authorize(Roles = "user")]
        [HttpPost]
        public ActionResult EditAds(Ad ad, HttpPostedFileBase uploadImage1, HttpPostedFileBase uploadImage2,
                                              HttpPostedFileBase uploadImage3, HttpPostedFileBase uploadImage4,
                                              HttpPostedFileBase uploadImage5, HttpPostedFileBase uploadImage6)
        {
            ad.AdDateAdd = DateTime.Now;
            ad.AdUserId = User.Identity.Name;

            if (ModelState.IsValid && uploadImage1 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage1.InputStream))
                {
                    ad.Image1 = binaryReader.ReadBytes(uploadImage1.ContentLength);
                }
            }
            if (ModelState.IsValid && uploadImage2 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage2.InputStream))
                {
                    ad.Image2 = binaryReader.ReadBytes(uploadImage2.ContentLength);
                }
            }
            if (ModelState.IsValid && uploadImage3 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage3.InputStream))
                {
                    ad.Image3 = binaryReader.ReadBytes(uploadImage3.ContentLength);
                }
            }
            if (ModelState.IsValid && uploadImage4 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage4.InputStream))
                {
                    ad.Image4 = binaryReader.ReadBytes(uploadImage4.ContentLength);
                }
            }
            if (ModelState.IsValid && uploadImage5 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage5.InputStream))
                {
                    ad.Image5 = binaryReader.ReadBytes(uploadImage5.ContentLength);
                }
            }
            if (ModelState.IsValid && uploadImage6 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage6.InputStream))
                {
                    ad.Image6 = binaryReader.ReadBytes(uploadImage6.ContentLength);
                }
            }

            db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Authorize(Roles = "user")]
        public ActionResult MyAds(int? page)
        {
            InitDropDownList();
            var list = db.Ads
              .Include(p => p.AdCat)
              .Include(p => p.AdLocation)
              .Include(p => p.AdType)
              .Where(p => p.AdUserId == User.Identity.Name)
              .OrderByDescending(p => p.AdDateUpdate);  
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(list.ToPagedList(pageNumber, pageSize));
        } 
        [HttpGet]
        [Authorize(Roles = "user")]
        public ActionResult ChangeAdStat(int? id, int? AdId)
        {
            if (AdId != null) id = AdId;
            //Установка позиции на выбранное поле 
            Ad ad = db.Ads.Find(id);
            if (ad != null)
            {
                if (ad.AdUserId == User.Identity.Name)
                {
                    if (ad.AdIsEnable == true)
                        ad.AdIsEnable = false;
                    else ad.AdIsEnable = true;
                    db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
                    db.SaveChanges();
                    return RedirectToAction("MyAds");
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        } 
        [HttpPost]
        [Authorize(Roles = "user")]
        public ActionResult MyAds(int? AdCatId, int? AdSortPriceId, int? AdSortDateId, int? AdTypeId, string searchAdHeader, int? page)
        {
            InitDropDownList();
            var list = Filter(AdCatId, AdSortPriceId, AdSortDateId, AdTypeId, searchAdHeader).Where(p => p.AdUserId == User.Identity.Name);
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(list.ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult SendComplaint()
        { 
            return View();
        }
        [HttpPost]
        public ActionResult SendComplaint(int AdId, AdUserdComplaint adComp)
        {
            adComp.AdUserName = User.Identity.Name;
            adComp.AdId = AdId;
            adComp.AdText =  Request.Form["couse"].ToString() + ":  " + adComp.AdText; 
            db.AdUserdComplaints.Add(adComp); 
            db.SaveChanges();// сохраняем в бд все изменения
            return RedirectToAction("Index");  
        }  
        public ActionResult UpdateAD(int? id, int? AdId)
        {
            if (AdId != null) id = AdId;
            //Установка позиции на выбранное поле 
            Ad ad = db.Ads.Find(id);
            if (ad != null)
            {
                if (ad.AdUserId == User.Identity.Name)
                {
                    if (DateTime.Now.Day - ad.AdDateUpdate.Value.Day >= 1)
                    { 
                        ad.AdDateUpdate = DateTime.Now;
                        db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
                        db.SaveChanges();
                    }
                    else ViewBag.NeedPayForUp = true;
                    return RedirectToAction("MyAds");
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        }

    
    }
}




