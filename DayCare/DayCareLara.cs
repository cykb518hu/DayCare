using DayCareDataModel;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DayCare
{
    public class DayCareLara
    {
        public List<DayCareModel> GetDataList()
        {
            var result = new List<DayCareModel>();
            var parentList = new List<CdcEntry>();
            var client = new RestClient("http://w1.lara.state.mi.us/ChildCareSearch/Home/SearchResults");
            var request = new RestRequest(Method.POST);

            request.AddParameter("pq_datatype", "JSON");
            request.AddParameter("pq_curpage", "1");
            request.AddParameter("pq_rpp", "600");
            request.AddParameter("pq_filter", "{\"mode\":\"AND\",\"data\":[{\"dataIndx\":\"CdcCnty\",\"value\":\"50\",\"condition\":\"equal\",\"dataType\":\"string\",\"cbFn\":\"\"}]}");
            var res = client.Execute(request);
            var resStr = JObject.Parse(res.Content);
            parentList = JsonConvert.DeserializeObject<List<CdcEntry>>(resStr["Data"].ToString());
            Console.WriteLine("get master data::" + parentList.Count);
            LogHelper.log.Info("get master data:" + parentList.Count);

            //var file = @"D:\Document\DayCare.json";
            //var json = File.ReadAllText(file);
            //parentList = JsonConvert.DeserializeObject<List<CdcEntry>>(JObject.Parse(json)["Data"].ToString());

            foreach (var r in parentList)
            {
                try
                {
                    var data = new DayCareModel();
                    data.FacilityInformation = new FacilityInfo();
                    data.DaysOpen = new DaysOpen();
                    data.LicenseeInformation = new LicenseeInfo();
                    data.LicenseInformation = new LicenseInfo();
                    data.ServicesOffered = new ServicesOffered();

                    data.FacilityInformation.Street = r.CdcAddr;
                    data.FacilityInformation.City = r.CdcCity;
                    data.FacilityInformation.State = "MI";
                    data.FacilityInformation.ZipCode = r.CdcZip;
                    data.FacilityInformation.County = r.CntyDesc;

                    data.LicenseeInformation.Name = r.CdcLicName;

                    data.LicenseInformation.Number = r.CdcLicNbr;
                    data.LicenseInformation.FacilityType = r.FacilityType;
                    var url = "http://w1.lara.state.mi.us/ChildCareSearch/Home/FacilityProfile/" + r.CdcLicNbr;
                    Console.WriteLine("get sub list:" + r.CdcLicNbr);

                    //var url = "http://59.110.217.147:8008/DG390077990.htm";
                    LogHelper.log.Info("ExtractDayCareDetailList:" + r.CdcLicNbr);
                    ExtractDayCareDetailList(url, data);
                    result.Add(data);
                }
                catch(Exception ex)
                {
                    LogHelper.log.Info("ExtractDayCareDetailList exception " + ex.ToString());
                }
            }

            return result;
        }
        public void ExtractDayCareDetailList(string url, DayCareModel data)
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection dayCareCenterNodes = doc.DocumentNode.SelectNodes("//div[@class='col-lg-6 profile']");
            //&#39;
            data.FacilityInformation.Name = RemoveStr(dayCareCenterNodes[0].InnerText);
            var address = RemoveStr(dayCareCenterNodes[1].InnerText);
            var index = address.IndexOf("Phone:");
            if (index > 0)
            {
                data.FacilityInformation.Phone = RemoveStr(address.Substring(index + 6));
            }
            else
            {
                data.FacilityInformation.Phone = "";
            }
            data.FacilityInformation.LicenseStatus = RemoveStr(dayCareCenterNodes[4].InnerText);


            data.LicenseInformation.EffectiveDate = RemoveStr(dayCareCenterNodes[6].InnerText);
            data.LicenseInformation.ExpirationDate = RemoveStr(dayCareCenterNodes[7].InnerText);
            data.LicenseInformation.Capacity = RemoveStr(dayCareCenterNodes[8].InnerText);
            data.LicenseInformation.PeriodOfOperation = RemoveStr(dayCareCenterNodes[9].InnerText);

            data.ServicesOffered.FullDayProgram = RemoveStr(dayCareCenterNodes[10].InnerText);
            data.ServicesOffered.Provides = RemoveStr(dayCareCenterNodes[11].InnerText);

            var licenseAddress = RemoveStr(dayCareCenterNodes[13].InnerText.Replace("<br />", "").Replace("<br/>", "").Replace("<br>", ""));
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            licenseAddress = regex.Replace(licenseAddress, " ");
            var licenseIndex = licenseAddress.IndexOf("Phone:");
            if (licenseIndex > 0)
            {
                data.LicenseeInformation.Address = licenseAddress.Substring(0, licenseIndex);
                data.LicenseeInformation.Phone = licenseAddress.Substring(licenseIndex + 6);
            }
            else
            {
                data.LicenseeInformation.Address = licenseAddress;
                data.LicenseeInformation.Phone = "";
            }

            HtmlNode DaysOpen = doc.DocumentNode.SelectSingleNode("//tr[@class='customerTable']");

            data.DaysOpen.Sunday = GetDayOpenStr(DaysOpen.SelectNodes("td")[0].InnerText);
            data.DaysOpen.Monday = GetDayOpenStr(DaysOpen.SelectNodes("td")[1].InnerText);
            data.DaysOpen.Tuesday = GetDayOpenStr(DaysOpen.SelectNodes("td")[2].InnerText);
            data.DaysOpen.Wednesday = GetDayOpenStr(DaysOpen.SelectNodes("td")[3].InnerText);
            data.DaysOpen.Thursday = GetDayOpenStr(DaysOpen.SelectNodes("td")[4].InnerText);
            data.DaysOpen.Friday = GetDayOpenStr(DaysOpen.SelectNodes("td")[5].InnerText);
            data.DaysOpen.Saturday = GetDayOpenStr(DaysOpen.SelectNodes("td")[6].InnerText);
        }
        public string RemoveStr(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return "";
            }
            return str.Replace("\r", "").Replace("\n", "").TrimEnd().TrimStart();
        }
        public string GetDayOpenStr(string str)
        {
            if(!string.IsNullOrEmpty(str))
            {
                var arr = str.Split(':');
                if(arr.Length>1)
                {
                    return arr[1].Trim().Replace("&amp;", "&");
                }
                else
                {
                    return "";
                }
            }
            return "";
        }
    }

    public class CdcEntry
    {
        public string CdcLicNbr { get; set; }
        public string CdcName { get; set; }
        public string CdcAddr { get; set; }
        public string CdcCity { get; set; }
        public string CdcZip { get; set; }
        public string CdcLicName { get; set; }
        public string CdcType { get; set; }
        public string CdcCnty { get; set; }
        public string CntyDesc { get; set; }
        public string FacilityType { get; set; }
    }
}
