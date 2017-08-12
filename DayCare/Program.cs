using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DayCare;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Microsoft.Office.Interop.Excel;
using log4net;
using DayCareDataModel;

namespace SheetsQuickstart
{
    class Program
    {


        static void Main(string[] args)
        {
          
            List<DayCareModel> list = new List<DayCareModel>();
            try
            {
                Console.WriteLine("Scrapte start -" + DateTime.Now.ToString("o"));
                ScapeDataByCountyOrZip();

                Console.WriteLine("Scrapte list start -" + DateTime.Now.ToString("o"));
                var detailList = GetAllDetailList();

                #region
                var detailList1 = new List<ScrapeSource>();
                var detailList2 = new List<ScrapeSource>();
                var detailList3 = new List<ScrapeSource>();

                int count = 0;
                if (detailList.Count > 10)
                {
                    count = detailList.Count / 3;
                    for (var r = 1; r <= 3; r++)
                    {
                        if (r == 1)
                        {
                            detailList1 = detailList.Take(count).ToList();
                        }
                        if (r == 2)
                        {
                            detailList2 = detailList.Skip(count).Take(count).ToList();
                        }
                        if (r == 3)
                        {
                            detailList3 = detailList.Skip(count * 2).ToList();
                        }
                    }
                }
                else
                {
                    detailList1 = detailList;
                }
                LogHelper.log.Info("Scrapte Data detail start");
                Console.WriteLine("Scrapte Data detail start -" + DateTime.Now.ToString("o"));
                Task<List<DayCareModel>> task1 = Task.Factory.StartNew(() => GetDetailData(detailList1));
                Task<List<DayCareModel>> task2 = Task.Factory.StartNew(() => GetDetailData(detailList2));
                Task<List<DayCareModel>> task3 = Task.Factory.StartNew(() => GetDetailData(detailList3));

                Task.WaitAll(task1, task2, task3);

                list.AddRange(task1.Result);
                list.AddRange(task2.Result);
                list.AddRange(task3.Result);
                #endregion

                Console.WriteLine("Scrapte Data detail finished-" + DateTime.Now.ToString("o"));
              

                list = CompareWithPrevList(list);

                LogHelper.log.Info("Compare data finished");
                Console.WriteLine("Compare data finished -" + DateTime.Now.ToString("o"));

                LogHelper.log.Info("update data to google start");
                GoogleSheetApi googleSheet = new GoogleSheetApi();
                googleSheet.CreateNewSheet(list);
                LogHelper.log.Info("update data to google finised");

                Console.WriteLine("Upload to google finished -" + DateTime.Now.ToString("o"));

                LogHelper.log.Info("All finised");
                Console.WriteLine("All finished -" + DateTime.Now.ToString("o"));
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                LogHelper.log.Info("exception:" + ex.ToString());
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
        public static void ScapeDataByCountyOrZip()
        {
            LogHelper.log.Info("ScapeDataByCountyOrZip start");
            var fileName = ConfigurationManager.AppSettings.Get("CountyZipFile").ToString();
            var json = File.ReadAllText(fileName);

            var list = JsonConvert.DeserializeObject<List<CountyZipModel>>(json);
            DayCareScrape me = new DayCareScrape("http://www.dleg.state.mi.us");
            foreach (var r in list)
            {
                if(r.UseCounty)
                {
                    r.County = r.County.ToUpper();
                    var url = string.Format("/brs_cdc/rs_lfl.asp?cdc_name=&address=&cnty_name={0}&cdc_city=&cdc_zip=&ftype=DC&lic_name=&lic_nbr=&Search=Search&sorry=yes", r.County);
                    if(!me.ExtractDayByCounty(url))
                    {
                        r.UseCounty = false;
                    }
                }
            }
            string nweJson = JsonConvert.SerializeObject(list);
            System.IO.File.WriteAllText(fileName, nweJson);
            LogHelper.log.Info("ScapeDataByCountyOrZip end");
        }
        public static List<ScrapeSource> GetAllDetailList()
        {
            LogHelper.log.Info("scrape data list start");
            List<ScrapeSource> list = new List<ScrapeSource>();
            var zipCodeList = new List<string>();
            var file = ConfigurationManager.AppSettings.Get("CountyZipFile").ToString();
            var json = File.ReadAllText(file);
            var countyZipList = JsonConvert.DeserializeObject<List<CountyZipModel>>(json);
            DayCareScrape me = new DayCareScrape("http://www.dleg.state.mi.us");

            foreach (var c in countyZipList)
            {
                if(c.UseCounty)
                {
                    Console.WriteLine("Scape by county:" + c.County);
                    try
                    {
                        var url = string.Format("/brs_cdc/rs_lfl.asp?cdc_name=&address=&cnty_name={0}&cdc_city=&cdc_zip=&ftype=DC&lic_name=&lic_nbr=&Search=Search&sorry=yes", c.County);
                        me.ExtractDayCareList(url, list, c.County, "");
                    }
                    catch(Exception ex)
                    {
                        LogHelper.log.Info("Scape by county:" + c.County + "exception:" + ex.ToString());
                    }
                    Console.WriteLine("Scape by county:" + c.County + "end...");
                }
                else
                {
                    foreach (var r in c.ZipCodeList)
                    {
                        Console.WriteLine("Scape by zipCode:" + r.ZipCode);
                        try
                        {
                            var url = string.Format("/brs_cdc/rs_lfl.asp?cdc_name=&address=&cnty_name=%25&cdc_city=&cdc_zip={0}&ftype=DC&lic_name=&lic_nbr=&Search=Search&sorry=yes", r.ZipCode.ToString());
                            me.ExtractDayCareList(url, list, c.County, r.ZipCode);
                        }
                        catch(Exception ex)
                        {
                            LogHelper.log.Info("Scape by zipCode:" + r.ZipCode + "exception:" + ex.ToString());
                        }
                        Console.WriteLine("Scape by zipCode:" + r.ZipCode + "end...");
                    }
                }
            }
            LogHelper.log.Info("scrape data list end");
            return list;
        }

        public static List<DayCareModel> GetDetailData(List<ScrapeSource> list)
        {
            var resultList = new List<DayCareModel>();
            DayCareScrape me = new DayCareScrape("http://www.dleg.state.mi.us");
            foreach (var r in list)
            {
                try
                {
                    Console.WriteLine(BuildLogMessage(r));
                    var result = me.ExtractDayCareDetailList("http://www.dleg.state.mi.us/brs_cdc/" + r.DetailUrl);
                    result.FacilityInformation.County = r.County;
                    resultList.Add(result);
                }
                catch (Exception ex)
                {
                    LogHelper.log.Info(BuildLogMessage(r)+ "exception:" + ex.ToString());
                    if (ex.Message.IndexOf("The operation has timed out") >= 0 || ex.Message.IndexOf("无法连接到远程服务器") >= 0)
                    {
                        try
                        {
                            Console.WriteLine(BuildLogMessage(r));
                            var result = me.ExtractDayCareDetailList("http://www.dleg.state.mi.us/brs_cdc/" + r.DetailUrl);
                            result.FacilityInformation.ZipCode = r.ToString();
                            resultList.Add(result);
                        }
                        catch (Exception subex)
                        {
                            LogHelper.log.Info(BuildLogMessage(r) + "exception:" + subex.ToString());
                        }
                    }

                }

                Console.WriteLine(BuildLogMessage(r) + "- End...");
            }

            return resultList;
        }
        public static string BuildLogMessage(ScrapeSource model)
        {
            var county = string.IsNullOrEmpty(model.County) ? "" : model.County;
            var zipCode = string.IsNullOrEmpty(model.ZipCode) ? "" : model.ZipCode;
            return string.Format("County:{0}-ZipCode{1}-Detail{2}", county, zipCode, model.DetailUrl);

        }

        public static List<DayCareModel> CompareWithPrevList(List<DayCareModel> list)
        {
            try
            {
                var file = ConfigurationManager.AppSettings.Get("PrevList").ToString();
                var json = File.ReadAllText(file);
                var prevList = JsonConvert.DeserializeObject<List<DayCareModel>>(json);

                string jsonnew = JsonConvert.SerializeObject(list.ToArray());
                System.IO.File.WriteAllText(file, jsonnew);
                if (prevList != null && prevList.Any())
                {
                    foreach (var r in list)
                    {
                        if (prevList.Any(x => x.LicenseInformation.Number.Equals(r.LicenseInformation.Number)))
                        {
                            continue;
                        }
                        r.FacilityInformation.Status = "New";
                    }
                    foreach (var r in prevList)
                    {
                        if (list.Any(x => x.LicenseInformation.Number.Equals(r.LicenseInformation.Number)))
                        {
                            continue;
                        }
                        r.FacilityInformation.Status = "Removed";
                        list.Add(r);
                    }
                }
                list = list.OrderByDescending(x => x.FacilityInformation.Status).ThenBy(x => x.FacilityInformation.ZipOrder).ToList();
            }
            catch(Exception ex)
            {
                LogHelper.log.Info("compare data exception:" + ex.ToString());
            }
            return list;
        }
        
    }
}