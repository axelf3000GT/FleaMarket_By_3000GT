using System;
using System.Collections.Generic;
using System.Linq;
using FleaMarket.Models;
using System.Web.UI.WebControls;
using System.Data.Entity;
using System.Web.Mvc;
using System.IO;
using System.Web;  
using PagedList;
using ImageResizer;

//namespace FleaMarket.Controllers
namespace FleaMarket.Controllers
{
    public class FleaMarketController : Controller
    {
        const int pageSize = 10, maxcountAD = 10;
        FleaContext db = new FleaContext();
        FleaContext dbNews = new FleaContext();
        public bool isNewlyEnrolled { get; set; }
        public void InitDropDownList()
        {
            //для DropDownList добавление значений 
            ViewBag.AdCat = new SelectList(db.AdCats, "AdCatId", "AdCatSubHeader");  
            ViewBag.AdLocation = new SelectList(db.AdLocations, "AdLocationId", "AdLocationArea");  
            ViewBag.AdType = new SelectList(db.AdTypes, "AdTypeId", "AdTypeName"); 
            //------------------------------
            var query = (from m in db.AdLocations
                         select m.AdLocationCity)
                        .Distinct();
            SelectList AdLocationCitys = new SelectList(query); 
            //------------------------------ 
            ViewBag.AdLocationCity = AdLocationCitys;
            //ViewBag.AdLocationCity = AdLocationCitys;
            //ViewBag.AdLocationCity = AdLocationCitys;
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
        public IQueryable<FleaMarket.Models.Ad> Filter(int? AdCatId, int? AdSortPriceId, int? AdSortDateId, int? AdTypeId, string searchAdTopic, string AdCity)
        {
            var list = db.Ads
                .Include(p => p.AdCat)
                    .Include(p => p.AdLocation)
                    .Include(p => p.AdType);
            //Объединение таблиц |
            if (AdCity != "")
            {
                list = list
                   .Where(p => p.AdTypeId == AdTypeId)
                   .Where(p => p.AdCity == AdCity)
                   .Where(p => p.AdIsEnable == true)
                   .Where(p => p.AdTopic.Contains(searchAdTopic) || p.AdText.Contains(searchAdTopic));
            }
            else
            {
                list = list
                    .Where(p => p.AdTypeId == AdTypeId)
                    .Where(p => p.AdIsEnable == true)
                    .Where(p => p.AdTopic.Contains(searchAdTopic) || p.AdText.Contains(searchAdTopic));
            }
            if (Request.Form["isAdTopic"].Contains("true"))
            {
                list = list
                    .Where(p => p.AdTopic.Contains(searchAdTopic));
            }
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
        
        public void DeletePicture(String picName)
        {
            String fullpath = Server.MapPath("~/content/images/" + picName);
            System.IO.File.Delete(fullpath);
        }
        public bool CheckAmoutAD()
        {
            var list = db.Ads
                             .Where(p => p.AdUserId == User.Identity.Name)
                             .Where(p => p.AdIsEnable == true)
                             .Count();
            if (list > maxcountAD)
            {
                return false;
            }
            return true;
        } 
        [HttpPost]
        public String Upload(HttpPostedFileBase upload, int mod)
        {
            var path = Server.MapPath("~/Content/Images/");
            upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
            String instr = "maxwidth=800&maxheight=600";
            String date = DateTime.Now.ToString("yyyyMMdd_HHmmss_");
            if (mod == 1)
            {
                instr = "maxwidth=150&maxheight=110";
                date += "_min_";
            }
            ImageBuilder.Current.Build(
                    new ImageJob(
                        upload.InputStream,
                        path + date + upload.FileName,
                        new Instructions(instr),
                        false,
                        false));
            return date + upload.FileName;
        } 
        [HttpGet]
        public ActionResult Index(int? Id, int? page)
        {
            var AdCatId = Id;
            
            InitDropDownList();
            //Объединение таблиц |
            var list = db.Ads
                .Include(p => p.AdCat)
                .Include(p => p.AdLocation)
                .Include(p => p.AdType)
                .Where(p => p.AdIsEnable == true);
            if (AdCatId != null && AdCatId != 1)
                list = list.Where(p => p.AdCatId == AdCatId);
            list = list.OrderByDescending(p => p.AdDateUpdate);
            ViewBag.AdCount = list.Count();
            //------------------------------ 
           

            int pageNumber = (page ?? 1);
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        //ГЛАВНАЯ СТРАНИЦА - ВСЕ ОБЪЯВЛЕНИЯ
        [HttpPost]
        public ActionResult Index(int? AdCatId, int? AdSortPriceId, int? AdSortDateId, int? AdTypeId, string searchAdTopic, int? page, string AdCity)
        {
            InitDropDownList();

            var list = Filter(AdCatId, AdSortPriceId, AdSortDateId, AdTypeId, searchAdTopic, AdCity);
            int pageNumber = (page ?? 1);
            ViewBag.AdCount = list.Count();
            return PartialView(list.ToPagedList(pageNumber, pageSize));
        }


        //ДОБАВЛЕНИЕ ОБЪЯВЛЕНИЯ
        //шаг1
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult AddNewAdsSelectCity()
        {
             
            if (CheckAmoutAD()) 
            {

                //---------выбор страны--------------------
                IEnumerable<AdLocation> adLocationCountry = db.AdLocations;
                var query = (from m in db.AdLocations
                             select m.AdLocationCountry)
                             .Distinct();
                SelectList AdLocationCountrys = new SelectList(query);
                ViewBag.AdLocationCountry = AdLocationCountrys;
                 
                //---------выбор города-----быдло код, кода стану умнее исправлю!-----
                ViewBag.AdLocationCity = new SelectList(new List<SelectListItem>{
                                 new SelectListItem {Selected = true, Text = "Минск", Value = "Минск"},
                                 }, "Value", "Text").ToList();
                var LocationCity = db.AdLocations.Select(m => m.AdLocationCity).Distinct().ToList();
                foreach (var item in LocationCity)
                {
                    if (item != "Минск" && item != null)
                        ViewBag.AdLocationCity.Add(new SelectListItem { Selected = false, Text = item, Value = item });
                }
                //-----------------------------------------
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
            else
            { 
                String MessageError = "Достигнуто максимально возможное количество объявлений. Чтобы добавить новое, необходимо деактивировать существующее.";
                return RedirectToAction("Err", "FleaMarket", new { MessageError });
            }
            
        }
        //REDITERCT TO шаг2
        [HttpPost]
        public ActionResult AddNewAdsSelectCity(Ad ad)
        {
            string country = Request.Form["AdLocationCountry"].ToString();
            string city = Request.Form["AdLocationCity"].ToString();
            string catHeader = Request.Form["AdCatHeader"].ToString();
            int type = Convert.ToInt16(Request.Form["AdType"]);
            return RedirectToAction("AddNewAds", "FleaMarket", new { city, country, type, catHeader });
        }

        //шаг2
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
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
        //шаг 2 - сохранение объявления
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
            ad.AdCity = city;
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
        //шаг 3 - добавление картинок
        [HttpGet]
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult AddNewAdsPictures()
        {
            return View();
        }
        //шаг 3 - сохранение картинок
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
                        ad.Image1min = Upload(uploadImage1, 1);
                        ad.Image1 = Upload(uploadImage1, 2);
                    }
                }
                if (ModelState.IsValid && uploadImage2 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage2.InputStream))
                    {
                        ad.Image2min = Upload(uploadImage2, 1);
                        ad.Image2 = Upload(uploadImage2, 2);
                    }
                }
                if (ModelState.IsValid && uploadImage3 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage3.InputStream))
                    {
                        ad.Image3min = Upload(uploadImage3, 1);
                        ad.Image3 = Upload(uploadImage3, 2);
                    }
                }
                if (ModelState.IsValid && uploadImage4 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage4.InputStream))
                    {
                        ad.Image4min = Upload(uploadImage4, 1);
                        ad.Image4 = Upload(uploadImage4, 2);
                    }
                }
                if (ModelState.IsValid && uploadImage5 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage5.InputStream))
                    {
                        ad.Image5min = Upload(uploadImage5, 1);
                        ad.Image5 = Upload(uploadImage5, 2);
                    }
                }
                if (ModelState.IsValid && uploadImage6 != null)
                {
                    using (var binaryReader = new BinaryReader(uploadImage6.InputStream))
                    {
                        ad.Image6min = Upload(uploadImage6, 1);
                        ad.Image6 = Upload(uploadImage6, 2);
                    }
                }
                db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
                db.SaveChanges();

            }
            return RedirectToAction("Index", "FleaMarket");
        }
        //ОПИСАНИЕ ОБЪЯВЛЕНИЯ
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

                if ((list.AdUserId == User.Identity.Name) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminall")))
                    ViewBag.FullAcess = true;

                if (list != null)
                    return View(list);
            }
            return HttpNotFound();
        }

        //РЕДАКТИРОВАНИЕ ОБЪЯВЛЕНИЯ 
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
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

                if ((ads.AdUserId == User.Identity.Name) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminall")))
                {
                    return View(ads);
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        }
        //сохранение отредактированного объявления
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        [HttpPost]
        public ActionResult EditAds(Ad ad, HttpPostedFileBase uploadImage1, HttpPostedFileBase uploadImage2,
                                              HttpPostedFileBase uploadImage3, HttpPostedFileBase uploadImage4,
                                              HttpPostedFileBase uploadImage5, HttpPostedFileBase uploadImage6,
                                              Boolean? CheckBoxuploadImage1, Boolean? CheckBoxuploadImage2, Boolean? CheckBoxuploadImage3,
                                              Boolean? CheckBoxuploadImage4, Boolean? CheckBoxuploadImage5, Boolean? CheckBoxuploadImage6)
        {
            ad.AdDateAdd = DateTime.Now;

            if (ModelState.IsValid && uploadImage1 != null && (CheckBoxuploadImage1 == false || CheckBoxuploadImage1 == null))
            {
                using (var binaryReader = new BinaryReader(uploadImage1.InputStream))
                {
                    ad.Image1min = Upload(uploadImage1, 1);
                    ad.Image1 = Upload(uploadImage1, 2);
                }
            }
            else if (CheckBoxuploadImage1 == true)
            {
                DeletePicture(ad.Image1min);
                DeletePicture(ad.Image1);
                ad.Image1min = null;
                ad.Image1 = null;
            }
            if (ModelState.IsValid && uploadImage2 != null && (CheckBoxuploadImage2 == false || CheckBoxuploadImage2 == null))
            {
                using (var binaryReader = new BinaryReader(uploadImage2.InputStream))
                {
                    ad.Image2min = Upload(uploadImage2, 1);
                    ad.Image2 = Upload(uploadImage2, 2);
                }
            }
            else if (CheckBoxuploadImage2 == true)
            {
                DeletePicture(ad.Image2min);
                DeletePicture(ad.Image2);
                ad.Image2min = null;
                ad.Image2 = null;
            }
            if (ModelState.IsValid && uploadImage3 != null && (CheckBoxuploadImage3 == false || CheckBoxuploadImage3 == null))
            {
                using (var binaryReader = new BinaryReader(uploadImage3.InputStream))
                {
                    ad.Image3min = Upload(uploadImage3, 1);
                    ad.Image3 = Upload(uploadImage3, 2);
                }
            }
            else if (CheckBoxuploadImage3 == true)
            {
                DeletePicture(ad.Image3min);
                DeletePicture(ad.Image3);
                ad.Image3min = null;
                ad.Image3 = null;
            }
            if (ModelState.IsValid && uploadImage4 != null && (CheckBoxuploadImage4 == false || CheckBoxuploadImage4 == null))
            {
                using (var binaryReader = new BinaryReader(uploadImage4.InputStream))
                {
                    ad.Image4min = Upload(uploadImage4, 1);
                    ad.Image4 = Upload(uploadImage4, 2);
                }
            }
            else if (CheckBoxuploadImage4 == true)
            {
                DeletePicture(ad.Image4min);
                DeletePicture(ad.Image4);
                ad.Image4min = null;
                ad.Image4 = null;
            }
            if (ModelState.IsValid && uploadImage5 != null && (CheckBoxuploadImage5 == false || CheckBoxuploadImage5 == null))
            {
                using (var binaryReader = new BinaryReader(uploadImage5.InputStream))
                {
                    ad.Image5min = Upload(uploadImage5, 1);
                    ad.Image5 = Upload(uploadImage5, 2);
                }
            }
            else if (CheckBoxuploadImage5 == true)
            {
                DeletePicture(ad.Image5min);
                DeletePicture(ad.Image5);
                ad.Image5min = null;
                ad.Image5 = null;
            }
            if (ModelState.IsValid && uploadImage6 != null && (CheckBoxuploadImage6 == false || CheckBoxuploadImage6 == null))
            {
                using (var binaryReader = new BinaryReader(uploadImage6.InputStream))
                {
                    ad.Image6min = Upload(uploadImage6, 1);
                    ad.Image6 = Upload(uploadImage6, 2);
                }
            }
            else if (CheckBoxuploadImage6 == true)
            {
                DeletePicture(ad.Image6min);
                DeletePicture(ad.Image6);
                ad.Image6min = null;
                ad.Image6 = null;
            }

            db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult ChangeAdStat(int? id, int? AdId)
        {
            if (AdId != null) id = AdId;
            //Установка позиции на выбранное поле 
            Ad ad = db.Ads.Find(id);
            if (ad != null)
            {
                if ((ad.AdUserId == User.Identity.Name) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminall")))
                {
                    if (ad.AdIsEnable == true)
                        ad.AdIsEnable = false;

                    else
                    {
                        if (CheckAmoutAD())
                          ad.AdIsEnable = true;
                        else
                        {
                            String MessageError = "Достигнуто максимально возможное количество активных объявлений. Чтобы активировать данное объявление, деактивируйте любое другое.";
                            return RedirectToAction("Err", "FleaMarket", new { MessageError });
                        }


                    }
                    db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
                    db.SaveChanges();
                    return RedirectToAction("MyAds");
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        }
        //УДАЛЕНИЕ ОБЪЯВЛЕНИЯ шаг1
        [HttpGet]
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult DelAd(int? id, int? AdId)
        {
            if (AdId != null) id = AdId;
            //Установка позиции на выбранное поле 
            Ad ad = db.Ads.Find(id);
            if (ad != null)
            {
                if ((ad.AdUserId == User.Identity.Name) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminall")))
                {
                    return View(ad);
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        }
        //УДАЛЕНИЕ ОБЪЯВЛЕНИЯ шаг2
        [HttpGet]
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult DelAdConfirmed(int? id, int? AdId)
        {
            if (AdId != null) id = AdId;
            //Установка позиции на выбранное поле 
            Ad ad = db.Ads.Find(id);
            if (ad != null)
            {
                if ((ad.AdUserId == User.Identity.Name) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminall")))
                {

                    db.Ads.Remove(ad);
                    db.SaveChanges(); 
                    return RedirectToAction("MyAds");
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        }
        //МОИ ОБЪВЛЕНИЯ
        [HttpGet]
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult MyAds(int? page)
        {
            InitDropDownList();
            var list = db.Ads
              .Include(p => p.AdCat)
              .Include(p => p.AdLocation)
              .Include(p => p.AdType)
              .Where(p => p.AdUserId == User.Identity.Name)
              .OrderByDescending(p => p.AdDateUpdate);
            int pageNumber = (page ?? 1);
            ViewBag.AdCount = list.Count();
            return View(list.ToPagedList(pageNumber, pageSize));
        }
        //МОИ ОБЪВЛЕНИЯ следующая страница
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        [HttpPost]
        public ActionResult MyAds(int? AdCatId, int? AdSortPriceId, int? AdSortDateId, int? AdTypeId, string searchAdTopic, int? page, String AdCity)
        {
            InitDropDownList();
            var list = Filter(AdCatId, AdSortPriceId, AdSortDateId, AdTypeId, searchAdTopic, AdCity).Where(p => p.AdUserId == User.Identity.Name);
            int pageNumber = (page ?? 1);
            ViewBag.AdCount = list.Count();
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        //ЗАПОЛНЕНИЯ ЖАЛОБЫ
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        [HttpGet]
        public ActionResult SendComplaint()
        {
            return View();
        }
        //ОТПРАВКА ЖАЛОБЫ
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        [HttpPost]
        public ActionResult SendComplaint(int AdId, AdUserdComplaint adComp)
        {
            adComp.AdUserName = User.Identity.Name;
            adComp.AdId = AdId;
            adComp.AdText = Request.Form["couse"].ToString() + ":  " + adComp.AdText;
            db.AdUserdComplaints.Add(adComp);
            db.SaveChanges();// сохраняем в бд все изменения
            return RedirectToAction("Index");
        }
        //ОБНОВЛЕНИЕ ОБЪЯВЛЕНИЯ
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult UpdateAD(int? id, int? AdId)
        {
            if (AdId != null) id = AdId;
            //Установка позиции на выбранное поле 
            Ad ad = db.Ads.Find(id);
            if (ad != null)
            {
                if (ad.AdUserId == User.Identity.Name || (User.IsInRole("superadminads")) || (User.IsInRole("superadminads")) || (User.IsInRole("superadminall")))
                {
                    if ((DateTime.Now.Year - ad.AdDateUpdate.Value.Year >= 1) || (DateTime.Now.Month - ad.AdDateUpdate.Value.Month >= 1) || (DateTime.Now.Day - ad.AdDateUpdate.Value.Day >= 1))
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
        //ФООРМА ОШИБОК
        public ActionResult Err(String MessageError)
        {
            ViewBag.MessageError = MessageError;
            return View();
        }

        
        //ГЛАВНАЯ СТРАНИЦА СО СПИСКОМ ВСЕХ КАТЕГОРИЙ
        public ActionResult TitelPage()
        {
            FleaContext db2 = new FleaContext();
            //РАЗДЕЛ НОВОСТИ
            string NewsLink = "";
            var list = db2.Newss.OrderByDescending(m => m.NewsDateAdd).Take(20).ToList();
            foreach (var item in list)
                NewsLink += "<h6>- <a href = \"/News/DescriptionNews/" + item.NewsId + "\"> " + item.NewsHeader + "</a> </h6>";
            ViewBag.NewsLink = NewsLink;


            //РАЗДЕЛ ОБЪЯЛЕНИЯ
           
            String res1 = "", res2 = "", AdCatHeader = "";
            int i = 1;
            bool mod2 = false;
            int count = db.AdCats.Select(m => m.AdCatHeader).Distinct().Count()/2 + 2; //чтобы более равномерно были расположены категории + 2
            var listCat = db.AdCats.OrderBy(p => p.AdCatHeader);// 
            foreach (var item in listCat)
            {
                if ((i < count || AdCatHeader == item.AdCatHeader) && mod2 == false)
                { 
                    if (AdCatHeader != item.AdCatHeader)
                    { 
                        res1 += " <h4 style=\"color: #039acf\">" + item.AdCatHeader + ": </h4>";
                        i++;
                    }
                    res1 += "<h6> <a href = \"/FleaMarket/index/" + item.AdCatId + "\"> " + item.AdCatSubHeader + "  <sup>" + db2.Ads.Where(m => m.AdCatId == item.AdCatId).Where(m => m.AdIsEnable == true).Count() + "</sup>" + "</a> </h6>";
                    AdCatHeader = item.AdCatHeader;
                }
                else
                {
                    if (AdCatHeader != item.AdCatHeader)
                    {
                        mod2 = true;
                        res2 += "<h4 style=\"color: #039acf\">" + item.AdCatHeader + ": </h4>";
                        i++;
                    }
                    res2 += "<h6> <a href = \"/FleaMarket/index/" + item.AdCatId+"\"> " + item.AdCatSubHeader + "  <sup>" + db2.Ads.Where(m => m.AdCatId == item.AdCatId).Where(m => m.AdIsEnable == true).Count() + "</sup>" + "</a> </h6>"; 
                    AdCatHeader = item.AdCatHeader;
                }
            }
            ViewBag.res1 =  res1;
            ViewBag.res2 = res2;
             


            
            return View();
        }


        public ActionResult DetailPhoneNumber(string AdPhoneNumber, int? id)
        {
            if (Request.IsAjaxRequest())
            { 
                Ad ad = db.Ads.Find(id); //инкрементируем счетчик просмотра номера
                if (ad != null)
                    ad.AdCountWatchingPhone++;
                db.Entry(ad).State = EntityState.Modified; //указание на то что запись существует
                db.SaveChanges();

                ViewBag.AdPhoneNumber = AdPhoneNumber;
                ViewBag.AdUserId = ad.AdUserId;
                ViewBag.AdCountWatchingPhone = ad.AdCountWatchingPhone; 
                return PartialView(); 
            }
            return RedirectToAction("Index");
        }

        public ActionResult HomePage()
        {
            var list = dbNews.Newss.OrderByDescending(m => m.NewsDateAdd).Take(22).ToList(); 
            return View(list);
        }



    }
}




