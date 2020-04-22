using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WebApplication1
{
    public class AdyenHttpServicecopy
    {
        private HtmlWeb web = new HtmlWeb();
        public string GetFinalHtml()
        {
            var listStr =  GetList();
            var result = GetResult(listStr);
            var finalHtml = GetHtml(result);
            return finalHtml;
        }
        public string GetList()
        {
            var url = "https://www.zeekbeek.com/DesktopModules/CloudLaw.WebServices/API/Solr/SearchMembers";
            int page = 2;
            var parameter = "{\"mtype\":\"good\",\"region\":\"MI\",\"page\":" + page + ",\"url\":\"https://www.zeekbeek.com/SBM/Search-Results#mtype=good&region=MI\",\"professionSetting\":\"Lawyer\",\"vinclude\":\"SBM\",\"mexclude\":\"\",\"sort\":\"geo\"}";
            string responseText = null;
            var httpWebRequest = GetHttpWebRequest(url, parameter);
            // ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseText = reader.ReadToEnd();
                }
            }

            return responseText;

        }

        public string GetHtml(UserResultData data)
        {
            var url = "https://www.zeekbeek.com/DesktopModules/ZB.UserProfile/API/ZBUserProfile/LVParseItem";
            var str = "";
            if(data!=null&&data.results!=null)
            {
                foreach(var res in data.results)
                {
                    str += res.userId + ",";
                }
                if(str.Length>0)
                {
                    str = str.Substring(0, str.Length - 1);
                }
  
            }

            var parameter = "{\"TemplateName\":\"SBM-Biz-Cards\",\"UserIds\":[34041,20435,24386,41790,43603,41051,28105,44272,20740,46211,25034,49840,19322,58117,66991,21940,60299,40768,51052,60749,51811,34487,65210,25031,130992],\"PortalId\":0,\"CurrUserId\":-1}";
            string responseText = null;
            var httpWebRequest = GetHttpWebRequest(url, parameter);
            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                }
            }
            File.AppendAllText(@"C:\IIS\test\log.txt", responseText);
            LogHelper.log.Info("update data to google start");
            LogHelper.log.Info(responseText);
            return responseText;
        }
        public UserResultData GetResult(string json)
        {
            //json = File.ReadAllText(@"C:\IIS\test\json.txt");
            return JsonConvert.DeserializeObject<UserResultData>(json);

        }
        public HttpWebRequest GetHttpWebRequest(string url,string parameter)
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

        public void ReadVCF()
        {
            vCardReader vCardReader = new vCardReader();
            var data = File.ReadAllText(@"C:\IIS\test\P69684.vcf");
            vCardReader.ParseLines(data);
        }

        public void ReadData(string html)
        {
            var laywerList = new List<LaywerModel>();
           // html = File.ReadAllText(@"C:\IIS\test\log.txt", Encoding.UTF8);
           // html = html.Replace(@"\\\", "");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
          
            var lawyerDivs = doc.DocumentNode.SelectNodes("//div[@class='row1 cloudlaw-profile user-menu-container square biz-card clearfix']");

            foreach (HtmlNode lawyerDiv in lawyerDivs)
            {
                var laywer = new LaywerModel();
                var laywerDetail = lawyerDiv.SelectSingleNode("//div[@class='col-md-7 user-details']");
                var h41=laywerDetail.SelectSingleNode(".//h4");
                laywer.Name = h41.InnerText.Trim();
                HtmlNode profileLink = laywerDetail.SelectSingleNode("//a[@zb-role='profile-link']");
                var url= profileLink.Attributes["href"].Value;
                HtmlDocument detailDoc = web.Load(url);
                ReadDetailData(detailDoc, laywer);

                //var ps = laywerDetail.SelectNodes("//div[@class='user-pad biz-address']/p");
                //foreach(HtmlNode p in ps)
                //{
                //    if(p.InnerText.Contains("Admission"))
                //    {
                //        laywer.DateOfAdmission = p.InnerText;
                //    }
                //    if (p.InnerText.Contains("Licensed "))
                //    {
                //        laywer.LicensedIn = p.InnerText;
                //    }
                //    if (p.InnerText.Contains("Reg"))
                //    {
                //        laywer.Registration = p.InnerText;
                //    }
                //}


                laywerList.Add(laywer);

            }

        }

        public void ReadDetailData(HtmlDocument doc, LaywerModel laywer)
        {
            //html = File.ReadAllText(@"C:\IIS\test\detail.txt", Encoding.UTF8);
    
           // var doc = new HtmlDocument();
           // doc.LoadHtml(html);

            var uls = doc.DocumentNode.SelectNodes("//ul[@class='list-group']");
            //var div= doc.DocumentNode.SelectSingleNode("//div[@class='panel-body profile-pa']");
            //if(div!=null)
            //{
            //    var alist = div.SelectNodes("//a[@class='tag tagdlg disabled']");
            //    foreach(HtmlNode practise in alist)
            //    {
            //        laywer.ParctiseArea += practise.InnerText + " | ";
            //    }
            //    if(laywer.ParctiseArea.Length>0)
            //    {
            //        laywer.ParctiseArea = laywer.ParctiseArea.Trim().Substring(0, laywer.ParctiseArea.Length - 1);
            //    }

            //}
            var metaDiv = doc.DocumentNode.SelectSingleNode("//div[@class='DnnModule DnnModule-ZBUserProfileViewer DnnModule-3386']");
            if (metaDiv != null)
            {
                var emailMeta = metaDiv.SelectSingleNode(".//meta[@itemprop='email']");
                if(emailMeta!=null)
                {
                    laywer.Email = emailMeta.Attributes["content"].Value;
                }
                var faxMeta = metaDiv.SelectSingleNode(".//meta[@itemprop='faxNumber']");
                if (faxMeta != null)
                {
                    laywer.Fax = faxMeta.Attributes["content"].Value;
                }
                var telephoneMeta = metaDiv.SelectSingleNode(".//meta[@itemprop='telephone']");
                if (telephoneMeta != null)
                {
                    laywer.Telphone = telephoneMeta.Attributes["content"].Value;
                }
            }
            foreach (HtmlNode ul in uls)
            {
                HtmlNode org = ul.SelectSingleNode(".//span[@itemtype='http://schema.org/Place']");
                if (org != null)
                {
                    laywer.Org = org.InnerText.Trim();
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
            
                var lis =ul.SelectNodes(".//li[@class='list-group-item']");
                foreach(HtmlNode li in lis)
                {
                
                    if (li.InnerText.Contains("County"))
                    {
                        laywer.County = li.InnerText.Replace("County:", "").Trim();
                    }
                    if (li.InnerText.Contains("Country"))
                    {
                        laywer.Country = li.InnerText.Replace("Country:", "").Trim();
                    }
                    if (string.IsNullOrEmpty(laywer.Telphone))
                    {
                        if (li.InnerText.Contains("(C)")|| li.InnerText.Contains("(T)"))
                        {
                            laywer.Telphone = li.InnerText.Replace("(C)", "").Replace("(T)", "").Trim();
                        }
                    }
                    if (li.InnerText.ToLower().Contains("registration") || li.InnerText.ToLower().Contains("admission"))
                    {
                        var text = li.InnerHtml;
                        foreach (var str in Regex.Split(text, "<br>"))
                        {
                            if(str.Contains("State of Admission")|| str.Contains("Licensed"))
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


        public void ReadDetailDataNew(string html, LaywerModel laywer)
        {
            html = File.ReadAllText(@"C:\IIS\test\detail.txt", Encoding.UTF8);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var uls = doc.DocumentNode.SelectNodes("//ul[@class='list-group']");
            //var div= doc.DocumentNode.SelectSingleNode("//div[@class='panel-body profile-pa']");
            //if(div!=null)
            //{
            //    var alist = div.SelectNodes("//a[@class='tag tagdlg disabled']");
            //    foreach(HtmlNode practise in alist)
            //    {
            //        laywer.ParctiseArea += practise.InnerText + " | ";
            //    }
            //    if(laywer.ParctiseArea.Length>0)
            //    {
            //        laywer.ParctiseArea = laywer.ParctiseArea.Trim().Substring(0, laywer.ParctiseArea.Length - 1);
            //    }

            //}

            foreach (HtmlNode ul in uls)
            {
                HtmlNode org = ul.SelectSingleNode(".//span[@itemtype='http://schema.org/Place']");
                if (org != null)
                {
                    laywer.Org = org.InnerText.Trim();
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
                //HtmlNode tel = ul.SelectSingleNode(".//a[@zb-role='phone-vpl-link']");
                //if (tel != null)
                //{
                //    laywer.Tel = tel.InnerText.Trim();
                //}

                HtmlNode web = ul.SelectSingleNode(".//a[@zb-role='web-vpl-link']");
                if (web != null)
                {
                    laywer.WebUrl = web.InnerText.Trim();
                }

                var lis = ul.SelectNodes(".//li[@class='list-group-item']");
                foreach (HtmlNode li in lis)
                {
                    //HtmlNode job = li.SelectSingleNode(".//div[@itemprop='jobTitle']");
                    //if (job != null)
                    //{
                    //    laywer.Org = li.InnerText.Replace(job.InnerText, " | " + job.InnerText).Trim().Replace("   ", "");
                    //}

                    if (li.InnerText.Contains("County"))
                    {
                        laywer.County = li.InnerText.Replace("County:", "").Trim();
                    }
                    if (li.InnerText.Contains("Country"))
                    {
                        laywer.Country = li.InnerText.Replace("Country:", "").Trim();
                    }
                    if (li.InnerText.Contains("(T)"))
                    {
                        laywer.Telphone = li.InnerText.Replace("(T)", "").Trim();
                    }

                    if (li.InnerText.Contains("(F)"))
                    {
                        laywer.Fax = li.InnerText.Replace("(F)", "").Trim();
                    }

                    if (li.InnerHtml.Contains("Javascript"))
                    {

                    }

                    if (li.InnerText.ToLower().Contains("registration") || li.InnerText.ToLower().Contains("admission"))
                    {
                        var text = li.InnerHtml;
                        foreach (var str in Regex.Split(text, "<br>"))
                        {
                            if (str.Contains("State of Admission") || str.Contains("Licensed"))
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
    }


}