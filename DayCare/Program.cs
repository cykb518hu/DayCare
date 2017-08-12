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

namespace SheetsQuickstart
{
    class Program
    {

        static void Main(string[] args)
        {

            List<DayCareModel> list = new List<DayCare.DayCareModel>();
            try
            {
                var zipCodeList = new List<string>();
                var file = ConfigurationManager.AppSettings.Get("zipPath").ToString();
                var json = File.ReadAllText(file);
                var jobj = JArray.Parse(json);
                foreach (var r in jobj)
                {
                    zipCodeList.Add(r["ZipCode"].ToString());
                }
                #region
                var zipCodeList1 = new List<string>();
                var zipCodeList2 = new List<string>();
                var zipCodeList3 = new List<string>();

                int count = 0;
                if (zipCodeList.Count > 10)
                {
                    count = zipCodeList.Count / 3;
                    for (var r = 1; r <= 3; r++)
                    {
                        if (r == 1)
                        {
                            zipCodeList1 = zipCodeList.Take(count).ToList();
                        }
                        if (r == 2)
                        {
                            zipCodeList2 = zipCodeList.Skip(count).Take(count).ToList();
                        }
                        if (r == 3)
                        {
                            zipCodeList3 = zipCodeList.Skip(count * 2).ToList();
                        }
                    }
                }
                else
                {
                    zipCodeList1 = zipCodeList;
                }
                LogHelper.log.Info("Scrapte Data start");
                Console.WriteLine("Scrapte Data start -" + DateTime.Now.ToString("o"));
                Task<List<DayCareModel>> task1 = Task.Factory.StartNew(() => GetData(zipCodeList1));
                Task<List<DayCareModel>> task2 = Task.Factory.StartNew(() => GetData(zipCodeList2));
                Task<List<DayCareModel>> task3 = Task.Factory.StartNew(() => GetData(zipCodeList3));

                Task.WaitAll(task1, task2, task3);

                list.AddRange(task1.Result);
                list.AddRange(task2.Result);
                list.AddRange(task3.Result);
                #endregion

                Console.WriteLine("Scrapte Data finished-" + DateTime.Now.ToString("o"));
                LogHelper.log.Info("Scrapte Data finished");

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


        public static List<DayCareModel> GetData(List<string> zipCodelist, int times = 0)
        {
            var resultList = new List<DayCareModel>();
            DayCareScrape me = new DayCareScrape("http://www.dleg.state.mi.us");
            foreach (var r in zipCodelist)
            {
                try
                {
                    Console.WriteLine("Start ZipCode:" + r.ToString());
                    var url = string.Format("/brs_cdc/rs_lfl.asp?cdc_name=&address=&cnty_name=%25&cdc_city=&cdc_zip={0}&ftype=DC&lic_name=&lic_nbr=&Search=Search&sorry=yes", r.ToString());

                    var dic = me.ExtractDayCareList(url);
                    foreach (var d in dic)
                    {
                        try
                        {
                            Console.WriteLine(r + "-detail:" + d.Value);
                            var result = me.ExtractDayCareDetailList("http://www.dleg.state.mi.us/brs_cdc/" + d.Value);
                            result.FacilityInformation.ZipCode = r.ToString();
                            resultList.Add(result);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.log.Info(r + "-detail:" + d.Value + "exception:" + ex.ToString());
                            if (ex.Message.IndexOf("The operation has timed out") >= 0 || ex.Message.IndexOf("无法连接到远程服务器") >= 0)
                            {
                                #region if time out check again
                                try
                                {
                                    Console.WriteLine(r + "-detail:" + d.Value);
                                    var result = me.ExtractDayCareDetailList("http://www.dleg.state.mi.us/brs_cdc/" + d.Value);
                                    result.FacilityInformation.ZipCode = r.ToString();
                                    resultList.Add(result);
                                }
                                catch (Exception subex)
                                {
                                    LogHelper.log.Info(r + "-detail:" + d.Value + "exception:" + subex.ToString());
                                }
                                #endregion
                            }
                           
                        }
                    }
                    Console.WriteLine(r + "-This ZipCode End...");
                }
                catch (Exception ex)
                {
                    LogHelper.log.Info("ZipCode:" + r + "exception:" + ex.ToString());
                    if (ex.Message.IndexOf("The operation has timed out") >= 0|| ex.Message.IndexOf("无法连接到远程服务器") >= 0)
                    {
                        #region if time out check again
                        try
                        {
                            Console.WriteLine("Start ZipCode:" + r.ToString());
                            var url = string.Format("/brs_cdc/rs_lfl.asp?cdc_name=&address=&cnty_name=%25&cdc_city=&cdc_zip={0}&ftype=DC&lic_name=&lic_nbr=&Search=Search&sorry=yes", r.ToString());
                            var dic = me.ExtractDayCareList(url);
                            foreach (var d in dic)
                            {
                                try
                                {
                                    Console.WriteLine(r + "-detail:" + d.Value);
                                    var result = me.ExtractDayCareDetailList("http://www.dleg.state.mi.us/brs_cdc/" + d.Value);
                                    result.FacilityInformation.ZipCode = r.ToString();
                                    resultList.Add(result);
                                }
                                catch (Exception subex)
                                {
                                    LogHelper.log.Info(r + "-detail:" + d.Value + "exception:" + subex.ToString());
                                    if (ex.Message.IndexOf("The operation has timed out") >= 0)
                                    {
                                        #region if time out check again
                                        try
                                        {
                                            Console.WriteLine(r + "-detail:" + d.Value);
                                            var result = me.ExtractDayCareDetailList("http://www.dleg.state.mi.us/brs_cdc/" + d.Value);
                                            result.FacilityInformation.ZipCode = r.ToString();
                                            resultList.Add(result);
                                        }
                                        catch (Exception subsubex)
                                        {
                                            LogHelper.log.Info(r + "-detail:" + d.Value + "exception:" + subsubex.ToString());
                                        }
                                        #endregion
                                    }
                                    
                                }
                            }
                           
                            Console.WriteLine(r + "-This ZipCode End...");
                        }
                        catch(Exception tiemoutEx)
                        {
                            LogHelper.log.Info("ZipCode:" + r + "exception:" + tiemoutEx.ToString());
                        }
                        #endregion
                    }
                    
                }
            }
            return resultList;
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