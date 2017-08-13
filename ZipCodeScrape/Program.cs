
using DayCareDataModel;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipCodeScrape
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = "480353652";
            if (str.Length > 5)
            {
                str = str.Substring(0, 5);
            }
            return;
            var countyZip = new CountyZipModel();
            var countyCode = ConfigurationManager.AppSettings.Get("countyCode").ToString();

            countyZip.CountyCode = countyCode;
            countyZip.County = ConfigurationManager.AppSettings.Get("county").ToString().ToUpper();
            countyZip.ZipCodeList = new List<ZipCodeModel>();
            var url = string.Format("http://www.bestplaces.net/find/zip.aspx?st=mi&county={0}", countyCode);
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[contains(@href,'/zip-code/michigan/')]");
            foreach (HtmlNode node in nodes)
            {
                var zipCodeEntry = new ZipCodeModel();
                var zipCode = node.InnerText;
                zipCode = zipCode.Remove(zipCode.IndexOf("(")).Trim();
                zipCodeEntry.ZipCode = zipCode;
                countyZip.ZipCodeList.Add(zipCodeEntry);
            }
            countyZip.UseCounty = true;
            var fileName = ConfigurationManager.AppSettings.Get("CountyZipFile").ToString();
            var json = File.ReadAllText(fileName);

            var oldList = JsonConvert.DeserializeObject<List<CountyZipModel>>(json);
            if(oldList==null)
            {
                oldList = new List<CountyZipModel>();
                oldList.Add(countyZip);
            }
            var firstItem = oldList.FirstOrDefault(x => x.CountyCode == countyZip.CountyCode);
            if (firstItem == null)
            {
                oldList.Add(countyZip);
            }
            string nweJson = JsonConvert.SerializeObject(oldList);
            System.IO.File.WriteAllText(fileName, nweJson);

        }
    }


}
