using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipCodeScrape
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePath = ConfigurationManager.AppSettings.Get("ZipCodePath").ToString();
            var url = "http://www.bestplaces.net/find/zip.aspx?st=mi&county=26163";
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[contains(@href,'/zip-code/michigan/')]");
            var zipCode = string.Empty;
            var zipCodeList = new List<ZipCodeModel>();
            foreach (HtmlNode node in nodes)
            {
                zipCode = node.InnerText;
                zipCode = zipCode.Remove(zipCode.IndexOf("(")).Trim();
                zipCodeList.Add(new ZipCodeModel { ZipCode = zipCode });
            }
            string json = JsonConvert.SerializeObject(zipCodeList.ToArray());

            var fileName = filePath + "WayneCounty.json";
            System.IO.File.WriteAllText(fileName, json);
        }
    }

    public class ZipCodeModel
    {
        public string ZipCode { get; set; }
    }
}
