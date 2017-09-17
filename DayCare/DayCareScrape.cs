using DayCareDataModel;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCare
{
    public class DayCareScrape
    {
        private HtmlWeb web = new HtmlWeb();
        private string home = string.Empty;
        public DayCareScrape(string url)
        {
            home = url;
        }

        public bool ExtractDayByCounty(string query)
        {
            var url = home + query;
            HtmlDocument doc = web.Load(url);
            if(doc.DocumentNode.InnerText.IndexOf("Records Found : 200")>0)
            {
                return false;
            }
            return true;
        }
        public void ExtractDayCareList(string query, List<ScrapeSource> list, string county,string zipCode)
        {
            var url = home + query;
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection dayCareCenterNodes = doc.DocumentNode.SelectNodes("//a[contains(@href,'CDC_LIC_NBR')]");

            if (dayCareCenterNodes != null)
            {
                foreach (HtmlNode node in dayCareCenterNodes)
                {
                    string href = node.Attributes["href"].Value;
                    if (list.Any(x => x.DetailUrl.Equals(href)))
                    {
                        continue;
                    }
                    var model = new ScrapeSource();
                    model.DetailUrl = href;
                    model.County = county;
                    model.ZipCode = zipCode;
                    list.Add(model);
                }
            }
            var nextPageNode = doc.DocumentNode.SelectSingleNode("//img[contains(@src,'next.gif')]");
            if (nextPageNode != null)
            {
               
                var nextUrl = nextPageNode.ParentNode.Attributes["href"].Value;
                LogHelper.log.Info("next page:" + nextUrl);
                ExtractDayCareList(nextUrl, list, county, zipCode);
            }
        }
        public DayCareModel ExtractDayCareDetailList(string url)
        {
            var model = new DayCareModel();
            model.FacilityInformation = new FacilityInfo();
            model.DaysOpen = new DaysOpen();
            model.LicenseeInformation = new LicenseeInfo();
            model.LicenseInformation = new LicenseInfo();
            model.ServicesOffered = new ServicesOffered();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection dayCareCenterNodes = doc.DocumentNode.SelectNodes("//div[@class='col-lg-6 profile']");

            foreach (HtmlNode table in dayCareCenterNodes)
            {
                var rows = table.SelectNodes("tr");
                var title = rows[0].InnerHtml.Replace("\r", "").Replace(" ", "").Replace("\n", "");
                if (title.IndexOf("FacilityInformation") > 0)
                {
                    var facilityInformation = new FacilityInfo();
                    facilityInformation.Name = rows[1].SelectNodes("td")[1].FirstChild.InnerText;
                    var addressFont = rows[2].SelectNodes("td")[1].SelectNodes("font");


                    facilityInformation.Street = addressFont[0].InnerText.Replace("\r", "").Replace("\n", "").Replace("&nbsp;", "").Trim();

                    var address = addressFont[3].InnerText;
                    facilityInformation.City = address.Split(',')[0].Trim();
                    var stateAndZip = address.Split(',')[1].Replace("&nbsp;", ",");

                    facilityInformation.State = stateAndZip.Split(',')[0].Trim();
                    facilityInformation.ZipCode = stateAndZip.Split(',')[1].Trim();
                    //if(facilityInformation.ZipCode.Length>5)
                    //{
                    //    facilityInformation.ZipCode = facilityInformation.ZipCode.Substring(0, 5);
                    //}
                    facilityInformation.ZipOrder = Convert.ToInt32(facilityInformation.ZipCode);

                    facilityInformation.County = rows[3].SelectNodes("td")[1].SelectSingleNode("font").InnerText;
                    facilityInformation.Phone = rows[4].SelectNodes("td")[1].SelectSingleNode("font").InnerText;
                    facilityInformation.LicenseStatus = rows[4].SelectNodes("td")[3].SelectSingleNode("font").InnerText;
                    facilityInformation.Status = "";
                    model.FacilityInformation = facilityInformation;
                }
                if (title.IndexOf("LicenseeInfo") > 0)
                {
                    var licenseeInfo = new LicenseeInfo();
                    licenseeInfo.Name = rows[1].SelectNodes("td")[1].FirstChild.InnerText;
                    licenseeInfo.Address = rows[2].SelectNodes("td")[1].FirstChild.InnerText.Replace("&nbsp;", "");
                    licenseeInfo.Phone = rows[3].SelectNodes("td")[1].FirstChild.InnerText.Replace("\r", "").Replace("\n", "").TrimEnd().TrimStart();
                    model.LicenseeInformation = licenseeInfo;
                }

                if (title.IndexOf("LicenseInfo") > 0)
                {
                    var licenseInfo = new LicenseInfo();
                    licenseInfo.Number = rows[2].SelectNodes("td")[0].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    licenseInfo.FacilityType = rows[2].SelectNodes("td")[1].SelectSingleNode("div").SelectSingleNode("font").InnerText.Replace("\r", "").Replace("\n", "").TrimEnd().TrimStart();
                    licenseInfo.Capacity = rows[2].SelectNodes("td")[2].SelectSingleNode("div").SelectSingleNode("font").InnerText;

                    licenseInfo.EffectiveDate = rows[2].SelectNodes("td")[3].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    licenseInfo.ExpirationDate = rows[2].SelectNodes("td")[4].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    licenseInfo.PeriodOfOperation = rows[2].SelectNodes("td")[5].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    model.LicenseInformation = licenseInfo;

                }
                if (title.IndexOf("DaysOpen") > 0)
                {
                    var daysOpen = new DaysOpen();
                    daysOpen.Sunday = rows[2].SelectNodes("td")[0].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    daysOpen.Monday = rows[2].SelectNodes("td")[1].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    daysOpen.Tuesday = rows[2].SelectNodes("td")[2].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    daysOpen.Wednesday = rows[2].SelectNodes("td")[3].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    daysOpen.Thursday = rows[2].SelectNodes("td")[4].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    daysOpen.Friday = rows[2].SelectNodes("td")[5].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    daysOpen.Saturday = rows[2].SelectNodes("td")[6].SelectSingleNode("div").SelectSingleNode("font").InnerText;
                    model.DaysOpen = daysOpen;
                }
                if (title.IndexOf("ServicesOffered") > 0)
                {
                    var servicesOffered = new ServicesOffered();
                    servicesOffered.FullDayProgram = rows[1].SelectNodes("td")[1].SelectSingleNode("font").InnerText;
                    servicesOffered.Provides = rows[2].SelectNodes("td")[1].SelectSingleNode("font").InnerText.Replace("\r", "").Replace("\n", "").TrimEnd().TrimStart();
                    model.ServicesOffered = servicesOffered;
                }

            }
            return model;

        }
    }
  
}
