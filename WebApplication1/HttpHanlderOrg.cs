using HtmlAgilityPack;
using Newtonsoft.Json;
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
    public class HttpHanlderOrg
    {
        private HtmlWeb web = new HtmlWeb();
        private CookieContainer _cookies = new CookieContainer();

        public void GetFinalHtml()
        {
            List<string> userIds = new List<string>();

           // FileIn
            var str = File.ReadAllText(@"C:\IIS\test\data\all.txt");
            userIds = JsonConvert.DeserializeObject<List<string>>(str);
            var listData = new List<LaywerModelGabar>();
            try
            {
                int startNumber = 50000;
                int total = 70000;

                while (startNumber < total && startNumber < userIds.Count)
                {
                    var laywer = new LaywerModelGabar();
                    laywer.userId = userIds[startNumber];
                    if (startNumber % 100 == 0)
                    {
                        File.AppendAllText(@"C:\IIS\test\normalLog.txt", "start Number:" + startNumber + "  start time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                    }
                    ReadFromVcard(laywer, userIds[startNumber]);
                    GetLaywerDetail(laywer, userIds[startNumber]);
                    listData.Add(laywer);
                    startNumber++;
                }
            }
            catch (Exception ex)
            {

                File.AppendAllText(@"C:\IIS\test\error.txt", "GetFinalHtml exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\test\error.txt", ex.ToString() + "\r\n");
            }
            finally
            {
                File.AppendAllText(@"C:\IIS\test\data.txt", JsonConvert.SerializeObject(listData));
            }

        }
   
        public void GetLaywerDetail(LaywerModelGabar laywer, string userId)
        {
            try
            {

                var url = "https://www.gabar.org/MemberSearchDetail.cfm?ID=" + userId;
                var doc = new HtmlDocument();
                doc = web.Load(url);

                var lawyerDivs = doc.DocumentNode.SelectNodes("//div[@class='course-box-content']");
                if (lawyerDivs[1] != null)
                {
                    var lawyerDiv = lawyerDivs[1];
                    var rows = lawyerDiv.SelectNodes(".//tr");
                    foreach (HtmlNode row in rows)
                    {
                        if (row.SelectNodes(".//td") != null)
                        {
                            if (row.SelectNodes(".//td")[0].InnerText.Contains("Status"))
                            {
                                laywer.Status = row.SelectNodes(".//td")[1]?.InnerText.Trim();
                            }
                            if (row.SelectNodes(".//td")[0].InnerText.Contains("Admit Date"))
                            {
                                laywer.AdmitDate = row.SelectNodes(".//td")[1]?.InnerText.Trim();
                            }
                            if (row.SelectNodes(".//td")[0].InnerText.Contains("Law School"))
                            {
                                laywer.LawSchool = row.SelectNodes(".//td")[1]?.InnerText.Trim();
                            }
                            if (row.SelectNodes(".//td")[0].InnerText.Contains("Public Discipline"))
                            {
                                laywer.PublicDisciplne = row.SelectNodes(".//td")[1]?.InnerText.Trim();
                            }
                            if (row.SelectNodes(".//td")[0].InnerText.Contains("Member of "))
                            {
                                laywer.MemberOf = row.SelectNodes(".//td")[1]?.InnerText.Trim();
                            }
                        }
                    }
                    laywer.FullInfo = lawyerDiv.NextSibling.NextSibling.SelectSingleNode(".//a[@class='learn-more']")?.Attributes["href"]?.Value;

                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\IIS\test\error.txt", "GetLaywerList exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\test\error.txt", ex.StackTrace + "\r\n");
            }
        }


        public void ReadFromVcard(LaywerModelGabar laywer,string userId)
        {
            try
            {
                var html = "";
                var url = "https://www.gabar.org/customcf/generatevcard.cfm?ID=" + userId;
   
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        html = reader.ReadToEnd();
                    }
                }
                var dataArr = html.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
                foreach (var str in dataArr)
                {
                    if (str.StartsWith("N;LANGUAGE=en-us:"))
                    {
                        var name = str.Replace("N;LANGUAGE=en-us:", "").Split(';');
                        if (name.Length >= 3)
                        {
                            laywer.Surname = name[0];
                            laywer.GivenName = name[1];
                            laywer.MiddleName = name[2];
                        }
                    }
                    if (str.StartsWith("FN:"))
                    {
                        laywer.Name = str.Replace("FN:", ""); ;
                    }
                    if (str.StartsWith("ORG:"))
                    {
                        laywer.Org = str.Replace("ORG:", ""); ;
                    }
 
                    if (str.StartsWith("EMAIL;PREF;INTERNET:"))
                    {
                        laywer.Email = str.Replace("EMAIL;PREF;INTERNET:", "");
                    }
                    if (str.StartsWith("URL;WORK:"))
                    {
                        laywer.WebUrl = str.Replace("URL;WORK:", "");
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
                    if (str.StartsWith("ADR;WORK;PREF:"))
                    {
                        var address = str.Replace("ADR;WORK;PREF:", "").Split(';');
                        if (address.Length >= 8)
                        {
                            laywer.Street = address[2];
                            laywer.AddressLocality = address[4];
                            laywer.Region = address[5];
                            laywer.PostalCode = address[6];
                            laywer.Country = address[7];
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                File.AppendAllText(@"C:\IIS\test\error.txt", "ReadFromVcard exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\test\error.txt", ex.ToString() + "\r\n");
            }
            
        }


    }


    public class LaywerModelGabar
    {
        public string userId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public string MiddleName { get; set; }

        public string Status { get; set; }
        public string AdmitDate { get; set; }
        public string LawSchool { get; set; }
        public string PublicDisciplne { get; set; }
        public string MemberOf { get; set; }
        public string FullInfo { get; set; }
        //public string Title { get; set; }

        public string Telphone { get; set; }
        public string Cellphone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string WebUrl { get; set; }
       // public string ParctiseArea { get; set; }
        //public string County { get; set; }

        public string Country { get; set; }
        public string Street { get; set; }
        public string AddressLocality { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }

        public string Org { get; set; }
    }
}