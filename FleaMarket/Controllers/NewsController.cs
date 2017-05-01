using System;
using System.Collections.Generic;
using System.Linq;
using FleaMarket.Models;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using PagedList;
using System.Data.Entity;
using System.Web;
using System.IO;
using ImageResizer;

namespace FleaMarket.Controllers
{
   
    public class NewsController : Controller
    {
        const int pageSize = 10, maxcountAD = 10;
        FleaContext dbNews = new FleaContext();
        [HttpPost]
        public String Upload(HttpPostedFileBase upload, int mod)
        {
            var path = Server.MapPath("~/Content/Images/");
            upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
            String instr = "maxwidth=800&maxheight=600";
            String date = DateTime.Now.ToString("yyyyMMdd_HHmmss_");
            if (mod == 1)
            {
                instr = "maxwidth=220&maxheight=180";
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
        public void load(int? id)
        {
            //КАТЕГОРИИ 1-я колонка
            string NewsCatLink = "";
            var listCat = dbNews.NewsCats.OrderBy(m => m.NewsCatHeader).ToList();
            foreach (var item in listCat)
                NewsCatLink += "<p>- <a href = \"/News/index/" + item.NewsCatId + "\"> " + item.NewsCatHeader + "</a> </p>";
            ViewBag.NewsCatLink = NewsCatLink;

            //НОВОСТИ 2-я колонка
            string NewsLink = "<table>";  
            var listNews = dbNews.Newss.OrderByDescending(m => m.NewsDateAdd).Take(20).ToList();
            if (id != null)
                listNews = dbNews.Newss.Where(m => m.NewsCatId == id).OrderByDescending(m => m.NewsDateAdd).Take(20).ToList();
            foreach (var item in listNews)
                NewsLink += "<tr> <td style=\"vertical-align: top;\"><img src=\"/content/images/" + item.NewsImageLogoMini + "\" class=\"img-rounded\" style=\"width:80px;padding-right: 10px;\"></td>    <td><p><a href = \"/News/DescriptionNews/" + item.NewsId + "\"> "+ item.NewsHeader + " </ a ></ p > <br> <hr></td></tr> ";
 
            NewsLink += "</table>";
            ViewBag.NewsLink = NewsLink;
            
        }
        public ActionResult Index(int? id)
        {
            load(id); 
            return View();
        }
        //ПОЛНАЯ НОВОСТЬ
        [HttpGet]
        public ActionResult DescriptionNews(int? id)
        {
            if (id == null) id = Convert.ToInt32(TempData["NewsId"]);
            if (id != null)
            {
                var DescriptionNews = dbNews.Newss //объединяем таблички для просмотра 
                 .Where(p => p.NewsId == id)
                    .First();
                ViewBag.TextRaw = "<div class=\"text-justify\">" + DescriptionNews.NewsText + "</div>";
                ViewBag.Op = dbNews.Ops.Where(m => m.OpCat=="News" && m.OpCatId==id).OrderBy(m => m.OpDateAdd).ToList();
                
                load(DescriptionNews.NewsCatId);

                if (DescriptionNews != null)
                    return View(DescriptionNews);
            }
            return HttpNotFound();
        }
        [HttpPost]
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult DescriptionNews(int id, Op op)
        {
            if (id == null) id = Convert.ToInt32(TempData["NewsId"]);
            if (id != null)
            {
                
                if (Request.Form["Comment"].ToString() != "")
                { 
                    op.OpCat = "News";
                    op.OpCatId = id;
                    op.OpDateAdd = DateTime.Now;
                    op.OpUserName = User.Identity.Name;
                    op.OpText = Request.Form["Comment"].ToString(); 
                    dbNews.Ops.Add(op);
                    dbNews.SaveChanges();
                }
                var DescriptionNews = dbNews.Newss //объединяем таблички для просмотра 
                 .Where(p => p.NewsId == id)
                    .First();
                ViewBag.TextRaw = DescriptionNews.NewsText;
                ViewBag.Op = dbNews.Ops.Where(m => m.OpCat == "News" && m.OpCatId == id).OrderBy(m => m.OpDateAdd).ToList();

                load(DescriptionNews.NewsCatId);
                if (DescriptionNews != null)
                    return View(DescriptionNews);
            }
            return HttpNotFound();
        }


        
        [Authorize(Roles = "admin, user, webmaster, newseditor, superadminads, superadminall, superadminnews")]
        public ActionResult OpinionLike(int? Id)
        {
            if (Id != null)
            {
                    Op op = dbNews.Ops.Find(Id);
                    if (op != null && dbNews.Ops.Where(m => m.OpUserName == User.Identity.Name && m.OpId == Id).Count() == 0  )
                    {
                        
                        op.OpLikes++;
                        dbNews.Entry(op).State = EntityState.Modified; //указание на то что запись существует
                        dbNews.SaveChanges();
                        return RedirectToAction("DescriptionNews");
                    }
            }
            return RedirectToAction("DescriptionNews");
        }
        //ДОБАВЛЕНИЕ НОВОСТИ
        [Authorize(Roles = "adminAds, superadminall, superadminnews")] 
        [HttpGet]
        public ActionResult AddNewNews()
        {
            IEnumerable<NewsCat> newsCat = dbNews.NewsCats;
            SelectList NewsCat = new SelectList(newsCat, "NewsCatId", "NewsCatHeader");
            ViewBag.NewsCats = NewsCat; 
            return View();
        } 
         
        [ValidateInput(false)] 
        [HttpPost]
        public ActionResult AddNewNews(News news, HttpPostedFileBase uploadImage1)
        {   
            news.NewsDateAdd = DateTime.Now;
            news.NewsDateLastEditing = DateTime.Now;
            news.NewsUserId = User.Identity.Name;

            if (ModelState.IsValid && uploadImage1 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage1.InputStream))
                {
                    news.NewsImageLogoMini = Upload(uploadImage1, 1);
                    news.NewsImageLogo = Upload(uploadImage1, 2);
                }
            }


            dbNews.Newss.Add(news);
            dbNews.SaveChanges();
            return RedirectToAction("Index", "News");
        }
        //ПРОСМОТР СПИСКА РАЗМЕЩЕННЫХ НОВОСТЕЙ
        [Authorize(Roles = "adminAds, superadminall, superadminnews")] 
        public ActionResult ListNews(int? page, int? id)
        {  
            ViewBag.NewsCat = new SelectList(dbNews.NewsCats, "NewsCatId", "NewsCatHeader", "");
            var listNews = dbNews.Newss.Where(p => p.NewsCatId == 1).OrderByDescending(p => p.NewsDateAdd);
            if (User.Identity.Name == "adminAds")
                listNews = dbNews.Newss.Where(p => p.NewsUserId == User.Identity.Name).OrderByDescending(p => p.NewsDateAdd);
            int pageNumber = (page ?? 1); 
            return PartialView(listNews.ToList().ToPagedList(pageNumber, pageSize));
        }


        //выборка по категорий СПИСКА РАЗМЕЩЕННЫХ НОВОСТЕЙ
        [Authorize(Roles = "adminAds, superadminall, superadminnews")]
        public ActionResult ListNewsPart(int? page, int? id)
        {
            ViewBag.NewsCat = new SelectList(dbNews.NewsCats, "NewsCatId", "NewsCatHeader", "");
            var listNews = dbNews.Newss.Where(p => p.NewsCatId == id).OrderByDescending(p => p.NewsDateAdd);
            if (User.Identity.Name == "adminAds")
                listNews = dbNews.Newss.Where(p => p.NewsUserId == User.Identity.Name).Where(p => p.NewsCatId == id).OrderByDescending(p => p.NewsDateAdd);
            return PartialView(listNews);
        }

        //РЕДАКТИРОВАНИЕ НОВОСТИ
        [Authorize(Roles = "adminAds, superadminall, superadminnews")]
        [ValidateInput(false)]
        [HttpGet]
        public ActionResult EditNews(int id)
        {
            //Установка позиции на выбранное поле
            IEnumerable<NewsCat> newsCat = dbNews.NewsCats;
            SelectList NewsCat = new SelectList(newsCat, "NewsCatId", "NewsCatHeader");
            ViewBag.NewsCats = NewsCat; 
            News news = dbNews.Newss.Find(id);
            if (news != null)
            {

                if ((news.NewsUserId == User.Identity.Name) || (User.IsInRole("adminAds")) || (User.IsInRole("superadminall")) || (User.IsInRole("superadminnews")))
                {
                    return View(news);
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            return HttpNotFound();
        }
        //РЕДАКТИРОВАНИЕ НОВОСТИ - сохранение
        [Authorize(Roles = "adminAds, superadminall, superadminnews")]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditNews(News news, HttpPostedFileBase uploadImage1)
        {
            //Установка позиции на выбранное поле
            if (ModelState.IsValid && uploadImage1 != null)
            {
                using (var binaryReader = new BinaryReader(uploadImage1.InputStream))
                {
                    news.NewsImageLogoMini = Upload(uploadImage1, 1);
                    news.NewsImageLogo = Upload(uploadImage1, 2);
                }
            }
            dbNews.Entry(news).State = EntityState.Modified;
            dbNews.SaveChanges(); 
            return RedirectToAction("Index");
        }
        //УДАЛЕНИЕ НОВОСТИ
        [Authorize(Roles = "adminAds, superadminall, superadminnews")]
        [ValidateInput(false)]
        [HttpGet]
        public ActionResult DelNews(int id)
        { 
            if (id == null)
            {
                return HttpNotFound();
            }
            News copyNews = dbNews.Newss.Find(id);
            if (copyNews == null)
            {
                return HttpNotFound();
            }
            return View(copyNews);
            
        }
        [Authorize(Roles = "adminAds, superadminall, superadminnews")]
        [ValidateInput(false)]
        [HttpGet]
        public ActionResult DelNewsConfirmed(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            News copyNews = dbNews.Newss.Find(id);
            if (copyNews == null)
            {
                return HttpNotFound();
            }
            dbNews.Newss.Remove(copyNews);
            dbNews.SaveChanges();
            return RedirectToAction("Index"); 
        } 
    }  
}