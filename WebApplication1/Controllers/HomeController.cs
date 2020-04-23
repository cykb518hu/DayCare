using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //  var at = new AdyenHttpService();
            //LaywerModel laywer = new LaywerModel();
            // at.ReadFromVcard(laywer,"");
            // var at = new AdyenHttpService();
            // LaywerModel laywer = new LaywerModel();
            // at.ReadDetailData(null, laywer);
           // var at = new HttpHanlderOrg();
           // at.GetFinalHtml();
            return View();
        }

        public ActionResult About()
        {
       

            var at = new AdyenHttpService();
            at.DownLoadVCFFile();
            // var data = at.GetList();
            ViewBag.Message = "success";
         

            return View();
        }

        public ActionResult Contact()
        {
          //  var at = new AdyenHttpService();
          //  at.GetFinalHtml();
            LogHelper.log.Error("update data to google start");
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}