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

namespace WebApplication1222
{
    public class AdyenHttpService
    {
        private HtmlWeb web = new HtmlWeb();
        private CookieContainer _cookies = new CookieContainer();

        public void GetFinalHtml()
        {
            SaveVcard();
            return;
            var listData = new List<LaywerModel>();
            try
            {
                var str = GetHomePageCookie();
                if (string.IsNullOrEmpty(str))
                {
                    return;
                }
                int page = 1;
                int total = 10;
                while (page < total)
                {

                    File.AppendAllText(@"C:\IIS\test\normalLog.txt", "page:" + page + "  start time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                    //Thread.Sleep(2000);                    
                    var listStr = GetUserList(page);
                    var listHtml = GetListHtml(listStr);
                    listData.AddRange(GetLaywerList(listHtml));
                    File.AppendAllText(@"C:\IIS\test\normalLog.txt", "page:" + page + "  end time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                    page++;
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
                File.AppendAllText(@"C:\IIS\test\error.txt", "GetUserList exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "Page:" + page + "\r\n");
                File.AppendAllText(@"C:\IIS\test\error.txt", ex.ToString() + "\r\n");

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
                //File.AppendAllText(@"C:\IIS\test\log.txt", responseText);

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\IIS\test\error.txt", "GetListHtml exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\test\error.txt", ex.ToString() + "\r\n");
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

        public string GetHomePageCookie()
        {
            var url = "https://www.zeekbeek.com/";
            string html = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36";
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            _cookies = new CookieContainer();
            _cookies.Add(response.Cookies);

            var stream = response.GetResponseStream();

            //When you get the response from the website, the cookies will be stored
            //automatically in "_cookies".

            using (var reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            if (html.Contains("NOINDEX, NOFOLLOW"))
            {
                html = "";
            }
            return html;
        }

        public HtmlDocument GetPage(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36";
            //Set more parameters here...
            //...

            //This is the important part.
            request.CookieContainer = _cookies;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();

            //When you get the response from the website, the cookies will be stored
            //automatically in "_cookies".

            using (var reader = new StreamReader(stream))
            {
                string html = reader.ReadToEnd();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
        }

        public List<LaywerModel> GetLaywerList(string html)
        {

            var laywerList = new List<LaywerModel>();
            try
            {
                html = File.ReadAllText(@"C:\IIS\test\log.txt", Encoding.UTF8);
                html = html.Replace(@"\\\", "");
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var lawyerDivs = doc.DocumentNode.SelectNodes("//div[@class='row1 cloudlaw-profile user-menu-container square biz-card clearfix']");

                foreach (HtmlNode lawyerDiv in lawyerDivs)
                {
                    var laywer = new LaywerModel();
                    var laywerDetail = lawyerDiv.SelectSingleNode(".//div[@class='col-md-7 user-details']");
                    var h41 = laywerDetail.SelectSingleNode(".//h4");
                    laywer.Name = h41.InnerText.Trim();
                    HtmlNode profileLink = laywerDetail.SelectSingleNode(".//a[@zb-role='profile-link']");
                    var url = profileLink.Attributes["href"].Value;
                    url = "https://www.zeekbeek.com" + url;
                    var userId = profileLink.Attributes["userid"].Value;
                    laywer.userId = userId;
                    HtmlDocument detailDoc = GetPage(url);
                    if (ReadDetailData(detailDoc, laywer))
                    {
                        laywerList.Add(laywer);
                    }
                    else
                    {
                        //retry one time
                        Thread.Sleep(2000);
                        _cookies = new CookieContainer();
                        GetHomePageCookie();
                        detailDoc = GetPage(url);
                        laywerList.Add(laywer);
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\IIS\test\error.txt", "GetLaywerList exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\test\error.txt", ex.ToString() + "\r\n");
            }
            return laywerList;
        }


        public bool ReadDetailData(HtmlDocument doc, LaywerModel laywer)
        {
            //var html = File.ReadAllText(@"C:\IIS\test\detail.txt", Encoding.UTF8);
            // doc = new HtmlDocument();
            // doc.LoadHtml(html);

            var result = true;
            try
            {
                var uls = doc.DocumentNode.SelectNodes("//ul[@class='list-group']");
                if (uls != null)
                {
                    foreach (HtmlNode ul in uls)
                    {
                        HtmlNode org = ul.SelectSingleNode(".//span[@itemtype='http://schema.org/Place']");
                        if (org != null)
                        {
                            laywer.Org = org.InnerText.Trim();
                        }

                        HtmlNode title = ul.SelectSingleNode(".//div[@itemprop='jobTitle']");
                        if (title != null)
                        {
                            laywer.Title = title.InnerText.Trim();
                        }

                        HtmlNode addressLink = ul.SelectSingleNode(".//div[@itemprop='address']");
                        if (addressLink != null)
                        {
                            HtmlNode street = addressLink.SelectSingleNode(".//div[@class='btn-block']");
                            if (street != null)
                            {
                                laywer.Street = street.InnerText.Trim();
                            }
                            HtmlNode addressLocality = addressLink.SelectSingleNode(".//span[@itemprop='addressLocality']");
                            if (addressLocality != null)
                            {
                                laywer.AddressLocality = addressLocality.InnerText.Trim();
                            }
                            HtmlNode region = addressLink.SelectSingleNode(".//span[@itemprop='addressRegion']");
                            if (region != null)
                            {
                                laywer.Region = region.InnerText.Trim();
                            }
                            HtmlNode postalCode = addressLink.SelectSingleNode(".//span[@itemprop='postalCode']");
                            if (postalCode != null)
                            {
                                laywer.PostalCode = postalCode.InnerText.Trim();
                            }
                        }

                        HtmlNode web = ul.SelectSingleNode(".//a[@zb-role='web-vpl-link']");
                        if (web != null)
                        {
                            laywer.WebUrl = web.InnerText.Trim();
                        }

                        HtmlNode telphone = ul.SelectSingleNode(".//a[@zb-role='phone-vpl-link']");
                        if (telphone != null)
                        {
                            laywer.Telphone = telphone.InnerText.Trim();
                        }
                        var lis = ul.SelectNodes(".//li[@class='list-group-item']");
                        foreach (HtmlNode li in lis)
                        {

                            if (li.InnerText.Contains("County"))
                            {
                                laywer.County = li.InnerText.Replace("County:", "").Trim();
                            }
                            if (li.InnerText.Contains("Country"))
                            {
                                laywer.Country = li.InnerText.Replace("Country:", "").Trim();
                            }

                            if (li.InnerText.Contains("(C)") || li.InnerText.Contains("(T)"))
                            {
                                laywer.Cellphone = li.InnerText.Replace("(C)", "").Replace("(T)", "").Trim();
                            }
                            if (li.InnerText.Contains("(F)"))
                            {
                                laywer.Fax = li.InnerText.Replace("(F)", "").Trim();
                            }
                            if (li.InnerHtml.Contains("JavaScript"))
                            {
                                laywer.Email = ExtractEmails(li.InnerHtml);
                            }
                            if (li.InnerText.ToLower().Contains("registration") || li.InnerText.ToLower().Contains("admission"))
                            {
                                var text = li.InnerHtml;
                                foreach (var str in Regex.Split(text, "<br>"))
                                {
                                    if (str.Contains("State of Admission"))
                                    {
                                        laywer.LicensedIn = str.Replace("State of Admission:", "").Trim();
                                    }
                                    if (str.Contains("Registration"))
                                    {
                                        laywer.Registration = str.Replace("Registration #:", "").Trim();
                                    }
                                    if (str.Contains("Date of Admission"))
                                    {
                                        laywer.DateOfAdmission = str.Replace("Date of Admission:", "").Trim();
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\IIS\test\error.txt", "GetuserDetail exception:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
                File.AppendAllText(@"C:\IIS\test\error.txt", ex.ToString() + "\r\n");
            }
            return result;
        }

        public string ExtractEmails(string textToScrape)
        {
            var email = "";
            Regex reg = new Regex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}", RegexOptions.IgnoreCase);
            Match match;

            List<string> results = new List<string>();
            for (match = reg.Match(textToScrape); match.Success; match = match.NextMatch())
            {
                if (!(results.Contains(match.Value)))
                    results.Add(match.Value);
            }

            if (results.Count > 0)
            {
                email = results[0];
            }
            return email;
        }

        public void SaveVcard()
        {
            var url = "https://www.zeekbeek.com/vcard.ashx?userId=56752";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();

            //When you get the response from the website, the cookies will be stored
            //automatically in "_cookies".

            using (var reader = new StreamReader(stream))
            {
                var html = reader.ReadToEnd();
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
        public string DateOfAdmission { get; set; }
        public string LicensedIn { get; set; }
        public string Registration { get; set; }
        public string Title { get; set; }

        public string Telphone { get; set; }
        public string Cellphone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string WebUrl { get; set; }
        public string ParctiseArea { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public string AddressLocality { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }

        public string Org { get; set; }
    }
}