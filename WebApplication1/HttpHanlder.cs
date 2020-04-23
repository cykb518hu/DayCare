using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace WebApplication1
{
    public class AdyenHttpService
    {
        private HtmlWeb web = new HtmlWeb();

        public void GetFinalHtml()
        {
            var listData = new List<LaywerModel>();
            try
            {
                int page = 1;
                int total = 1840;
                while (page < total)
                {

                    File.AppendAllText(@"C:\IIS\zeekbeek\normalLog.txt", "page:" + page + "  start time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                    var listStr = GetUserList(page);
                    var listHtml = GetListHtml(listStr);
                    listData.AddRange(GetLaywerList(listHtml));
                    File.AppendAllText(@"C:\IIS\zeekbeek\normalLog.txt", "page:" + page + "  end time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                    page++;
                }
            }
            catch (Exception ex)
            {

                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", "GetFinalHtml exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", ex.ToString() + "\r\n");
            }
            finally
            {
                File.AppendAllText(@"C:\IIS\zeekbeek\data.txt", JsonConvert.SerializeObject(listData));
            }

        }

        public void DownLoadVCFFile()
        {
            // FileIn
           var  oldlaywerList = JsonConvert.DeserializeObject<List<LaywerModel>>(File.ReadAllText(@"C:\IIS\zeekbeek\40000-45000.txt"));
            try
            {
                int startNumber = 0;
       
                var folderName = @"C:\IIS\zeekbeek\vcf\30000\";
                while (startNumber < oldlaywerList.Count-1)
                {
                    var laywer = oldlaywerList[startNumber];
                    if (startNumber % 100 == 0)
                    {
                        File.AppendAllText(@"C:\IIS\zeekbeek\normalLog.txt", "start Number:" + startNumber + "  start time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                    }
                    var fileName = folderName + laywer.Registration + ".vcf";

                    if (File.Exists(fileName))
                    {
                        ReadFromVcard(laywer, fileName);
                        File.Delete(fileName);
                    }
                    else
                    {
                        File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", fileName + ": DownLoadVCFFile not found:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                    }
                    startNumber++;
                }
            }
            catch (Exception ex)
            {

                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", "DownLoadVCFFile exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", ex.ToString() + "\r\n");
            }
            finally
            {
                File.AppendAllText(@"C:\IIS\zeekbeek\subdata.txt", JsonConvert.SerializeObject(oldlaywerList));
            }
        }
        public string GetUserList(int page)
        {
            var listStr = "";
            try
            {
                var url = "https://www.zeekbeek.com/DesktopModules/CloudLaw.WebServices/API/Solr/SearchMembers";
                var parameter = "{\"mtype\":\"good\",\"region\":\"MI\",\"page\":" + page + ",\"url\":\"https://www.zeekbeek.com/SBM/Search-Results#mtype=good&region=MI\",\"professionSetting\":\"Lawyer\",\"vinclude\":\"SBM\",\"mexclude\":\"\",\"sort\":\"geo\"}";

                string responseText = null;
                var httpWebRequest = GetHttpWebRequest(url, parameter);
                using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                var userList = JsonConvert.DeserializeObject<UserResultData>(responseText);

                if (userList != null && userList.results != null)
                {
                    foreach (var res in userList.results)
                    {
                        listStr += res.userId + ",";
                    }
                    if (listStr.Length > 0)
                    {
                        listStr = listStr.Substring(0, listStr.Length - 1);
                    }

                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", "GetUserList exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "Page:" + page + "\r\n");
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", ex.ToString() + "\r\n");

            }
            return listStr;

        }

        public string GetListHtml(string idStr)
        {
            string responseText = null;
            try
            {
                var url = "https://www.zeekbeek.com/DesktopModules/ZB.UserProfile/API/ZBUserProfile/LVParseItem";

                var parameter = "{\"TemplateName\":\"SBM-Biz-Cards\",\"UserIds\":[" + idStr + "],\"PortalId\":0,\"CurrUserId\":-1}";

                var httpWebRequest = GetHttpWebRequest(url, parameter);
                using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                //File.AppendAllText(@"C:\IIS\zeekbeek\log.txt", responseText);

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", "GetListHtml exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", ex.ToString() + "\r\n");
            }
            return responseText;
        }
        public HttpWebRequest GetHttpWebRequest(string url, string parameter)
        {
            //Add default headers
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.UserAgent = "Mozilla/5.0 (compatible; Rigor/1.0.0; http://rigor.com)";
            Stream reqStream = null;
            byte[] postData = Encoding.UTF8.GetBytes(parameter);   //使用utf-8格式组装post参数
            reqStream = httpWebRequest.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            return httpWebRequest;
        }


        public List<LaywerModel> GetLaywerList(string html)
        {

            var laywerList = new List<LaywerModel>();
            try
            {                
                html = html.Replace(@"\\\", "");
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var lawyerDivs = doc.DocumentNode.SelectNodes("//div[@class='row1 cloudlaw-profile user-menu-container square biz-card clearfix']");
                if (lawyerDivs != null)
                {
                    foreach (HtmlNode lawyerDiv in lawyerDivs)
                    {
                        var laywer = new LaywerModel();
                        laywer.userId = lawyerDiv.Attributes["userid"]?.Value;
                        var laywerDetail = lawyerDiv.SelectSingleNode(".//div[@class='col-md-7 user-details']");
                        var h41 = laywerDetail.SelectSingleNode(".//h4");
                        laywer.Name = h41?.InnerText.Trim();
                        var ps = laywerDetail.SelectNodes(".//p");
                        
                        foreach (HtmlNode p in ps)
                        {
                            var str = p.InnerText;
                            if (str.Contains("Licensed"))
                            {
                                laywer.LicensedIn = str.Replace("Licensed In:", "").Trim();
                            }
                            if (str.Contains("Reg #"))
                            {
                                laywer.Registration = str.Replace("Reg #:", "").Trim();
                            }
                            if (str.Contains("Date of Admission"))
                            {
                                laywer.DateOfAdmission = str.Replace("Date of Admission:", "").Trim();
                            }
                            if (str.Contains("County:"))
                            {
                                laywer.County = str.Replace("County:", "").Trim();
                            }
                        }
                       // ReadFromVcard(laywer, laywer.userId);
                        laywerList.Add(laywer);
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", "GetLaywerList exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", ex.StackTrace + "\r\n");
            }
            return laywerList;
        }


        public void ReadFromVcard(LaywerModel laywer, string file)
        {
            try
            {
                var dataArr = File.ReadAllLines(file);
                foreach (var str in dataArr)
                {
                    if (str.StartsWith("N:"))
                    {
                        var name = str.Replace("N:", "").Split(';');
                        if (name.Length >= 3)
                        {
                            laywer.Surname = name[0];
                            laywer.GivenName = name[1];
                            laywer.MiddleName = name[2];
                        }
                    }
                  //  if (str.StartsWith("FN:"))
                  //  {
                    //    laywer.Name = str.Replace("FN:", "");
                   // }
                    if (str.StartsWith("ORG:"))
                    {
                        laywer.Org = str.Replace("ORG:", "").Replace(";", "");
                    }
                    if (str.StartsWith("TITLE:"))
                    {
                        laywer.Title = str.Replace("TITLE:", ""); ;
                    }
                    if (str.StartsWith("EMAIL;PREF;INTERNET:"))
                    {
                        laywer.Email = str.Replace("EMAIL;PREF;INTERNET:", "");
                    }
                    if (str.StartsWith("URL;WORK:"))
                    {
                        laywer.WebUrl = str.Replace("URL;WORK:", "");
                    }
                    if (str.StartsWith("URL;PREF;WORK:"))
                    {
                        laywer.ZeekBeekUrl = str.Replace("URL;PREF;WORK:", "");
                    }
                    if (str.StartsWith("TEL;WORK;FAX:"))
                    {
                        laywer.Fax = str.Replace("TEL;WORK;FAX:", "");
                    }
                    if (str.StartsWith("TEL;WORK;VOICE:"))
                    {
                        laywer.Telphone = str.Replace("TEL;WORK;VOICE:", "");
                    }
                    if (str.StartsWith("TEL;CELL;VOICE:"))
                    {
                        laywer.Cellphone = str.Replace("TEL;CELL;VOICE:", "");
                    }
                    if (str.StartsWith("ADR;PREF;WORK;PARCEL;ENCODING=QUOTED-PRINTABLE:"))
                    {
                        var address = str.Replace("ADR;PREF;WORK;PARCEL;ENCODING=QUOTED-PRINTABLE:", "").Split(';');
                        if (address.Length >= 7)
                        {
                            laywer.Street = address[2];
                            laywer.AddressLocality = address[3];
                            laywer.Region = address[4];
                            laywer.PostalCode = address[5];
                            laywer.Country = address[6];
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", "ReadFromVcard exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\zeekbeek\error.txt", ex.ToString() + "\r\n");
            }
            
        }


    }


    public class UserResultData
    {
       public List<UserIds> results { get; set; }
    }
    public class UserIds
    {
        public int userId { get; set; }
    }

    public class LaywerModel
    {
        public string userId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public string DateOfAdmission { get; set; }
        public string LicensedIn { get; set; }
        public string Registration { get; set; }
        public string Title { get; set; }

        public string Telphone { get; set; }
        public string Cellphone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ZeekBeekUrl { get; set; }
        public string WebUrl { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public string AddressLocality { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }

        public string Org { get; set; }
    }
}