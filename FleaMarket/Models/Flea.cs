using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Remoting.Messaging;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Web.Mvc;

namespace FleaMarket.Models
{
    public class Flea
    {
    }

    public class Ad
    {
        [HiddenInput(DisplayValue = false)]
        public int AdId { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int AdCatId { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int AdTypeId { get; set; }
        [Required(ErrorMessage = "Поле должно быть заполнено")]
        public String AdTopic { get; set; }
        [Required(ErrorMessage = "Поле должно быть заполнено")]
        [DataType(DataType.MultilineText)]
        public String AdText { get; set; }
        [Required(ErrorMessage = "Поле должно быть заполнено")]
        public double? AdCost { get; set; }
        [Required(ErrorMessage = "Поле должно быть заполнено")]
        public String AdPhoneNumber { get; set; }
        public String Image1 { get; set; }
        public String Image1min { get; set; }
        public String Image2 { get; set; }
        public String Image2min { get; set; }
        public String Image3 { get; set; }
        public String Image3min { get; set; }
        public String Image4 { get; set; }
        public String Image4min { get; set; }
        public String Image5 { get; set; }
        public String Image5min { get; set; }
        public String Image6 { get; set; }
        public String Image6min { get; set; }
        public int AdLocationId { get; set; }
        public String AdCity { get; set; }
        
        public DateTime AdDateAdd { get; set; }
        public DateTime? AdDateUpdate { get; set; }
        public DateTime? AdDateLastEditing { get; set; }
        public bool? AdIsEnable { get; set; }
        public String AdUserId { get; set; }
        public int? AdCountWatchngAd { get; set; }
        public int? AdCountWatchingPhone { get; set; }
        public String AdUserLastBuildingId { get; set; }
        public double? AdAuctionStartCost { get; set; }
        public double? AdAuctionLastCost { get; set; }
        public DateTime? AdAuctionDateEnd { get; set; }

        [ForeignKey("AdCatId")]
        public AdCat AdCat { get; set; }
        [ForeignKey("AdTypeId")]
        public AdType AdType { get; set; }
        [ForeignKey("AdLocationId")]
        public AdLocation AdLocation { get; set; }

    }

    public class AdCat
    {
        public int AdCatId { get; set; }
        public String AdCatHeader { get; set; }
        public String AdCatSubHeader { get; set; }
        public IEnumerable<Ad> Ads { get; set; }
    }
 
    public class AdType
    {
        public int AdTypeId { get; set; }
        public String AdTypeName { get; set; }
        public IEnumerable<Ad> Ads { get; set; }
    }
   
    public class AdUser
    {
        public int AdUserId { get; set; }
        public String AdUserName { get; set; }
        public int AdId { get; set; }
        public String AdUserRecall { get; set; }
        public String AdUserEvaluation { get; set; }
    }
    
    public class AdLocation
    {
        public int AdLocationId { get; set; }
        public String AdLocationCountry { get; set; }
        public String AdLocationCity { get; set; }
        public String AdLocationArea { get; set; }
        public IEnumerable<Ad> Ads { get; set; }
    }
    
    public class News
    {
        public int NewsId { get; set; }
        public int NewsCatId { get; set; } 
        public String NewsHeader { get; set; }
        public String NewsText { get; set; }
        public DateTime NewsDateAdd { get; set; }
        public DateTime? NewsDateLastEditing { get; set; }
        public String NewsUserId { get; set; }
        public String NewsImageLogoMini { get; set; }
        public String NewsImageLogo { get; set; }
        public String NewsTextDescription { get; set; }
        NewsCat NewsCat { get; set; }

    }
 
    public class NewsCat
    {
        public int NewsCatId { get; set; }
        public String NewsCatHeader { get; set; } 
        public IEnumerable<News> Newss { get; set; }
    }

    public class AdUserdComplaint
    {
        public int AdUserdComplaintId { get; set; }
        public String AdUserName { get; set; } 
        public int AdId { get; set; }
        [Required(ErrorMessage = "Поле должно быть заполнено")]
        [DataType(DataType.MultilineText)]
        public String AdText { get; set; } 
    }
    public class Op
    {
        public int OpId { get; set; }
        public String OpCat { get; set; }
        public int OpCatId { get; set; }
        public String OpUserName { get; set; } 
        public String OpText { get; set; }
        public int OpLikes { get; set; }
        public DateTime OpDateAdd { get; set; }
    }

}