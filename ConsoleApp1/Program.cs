using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildScript();
            //var url = string.Format("http://staging.peninsula.com/en/hong-kong/luxury-hotel-room-suite-types");
            //var web = new HtmlWeb();
            //HtmlDocument doc = web.Load(url);
            //HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='cardRoomDetails-cta']");
            //foreach (HtmlNode node in nodes)
            //{

            //}
            // for(int i=0;i<10; i++)
            //{
            // TestMalts();
          //  Test();

            //TestRedirect();
        }


        public static void TestMalts()
        {
            var url= "http://malts.diageoplatform.com/en-gb/visit-a-distillery?type=redirect";
            IWebDriver driver;
            ChromeOptions options = new ChromeOptions();
           // options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            driver = new ChromeDriver(@"C:\Program Files (x86)\Google\Chrome\Application", options);
            driver.Url = url;

            for (int i = 0; i < 10; i++)
            {
                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                executor.ExecuteScript("window.open('" + url + "')");
            }
        }

        public static void TestRedirect()
        {
          //  var url = "https://www.peninsula.com/en/make-a-booking?checkIn=&checkOut=&hotel=PHK&room=PHK-deluxe-room&r=sdfswer";

             var url = "http://pencm.peninsula.com/en/global-pages/Copy of Application Gateway Health Checking Page";

            Console.WriteLine(url);
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "HEAD";
           // request.AllowAutoRedirect = false;

            using (var response = request.GetResponse() as HttpWebResponse)
            {
               // var location2 = response.GetResponseHeader("Location");
              //  if (response.StatusCode== HttpStatusCode.Redirect)
               // {
              //      var location = response.GetResponseHeader("Location");
              //      Console.WriteLine(location);
             //   }
                Console.WriteLine(response.StatusCode);

            }

            //using (var httpClient = new HttpClient())
            //{

            //    var response = httpClient.GetAsync(url);
            //    if (response.Result.StatusCode == HttpStatusCode.Redirect)
            //    {
                    
            //    }
            //}

        }

        public static void MakeRequest()
        {
            var url = "https://www.peninsula.com/en/make-a-booking?checkIn=&checkOut=&hotel=PHK&room=PHK-deluxe-room&r=sdfswer";
            using (var client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip }))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response = client.SendAsync(request).Result;
                var statusCode = (int)response.StatusCode;

                // We want to handle redirects ourselves so that we can determine the final redirect Location (via header)
                if (statusCode >= 300 && statusCode <= 399)
                {
                    var redirectUri = response.Headers.Location;
                    if (!redirectUri.IsAbsoluteUri)
                    {
                        redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                    }
                }
                else if (!response.IsSuccessStatusCode)
                {
                    throw new Exception();
                }

            }
        }
        public static void GetAllPages()
        {
            IWebDriver driver;
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            driver = new ChromeDriver(@"C:\Program Files (x86)\Google\Chrome\Application", options);

            var staging = "http://pencd1.peninsula.com";
            var languageList = new List<string>();
            languageList.Add("en");
          //  languageList.Add("zh-cn");
           // languageList.Add("zh");
           // languageList.Add("ja");
           // languageList.Add("fr");
           // languageList.Add("es");
           // languageList.Add("pt");
           // languageList.Add("kr");

            var propertyList = new List<string>();

            propertyList.Add("hong-kong/special-offers/rooms/forbes-five-star-offer");
           // propertyList.Add("paris/5-star-luxury-hotel-16th-arrondissement");
           // propertyList.Add("tokyo/5-star-luxury-hotel-ginza");
           // propertyList.Add("shanghai/5-star-luxury-hotel-bund");
           // propertyList.Add("beijing/5-star-luxury-hotel-wangfujing");
           // propertyList.Add("new-york/5-star-luxury-hotel-midtown-nyc");
           // propertyList.Add("chicago/5-star-luxury-hotel-downtown-chicago");
           // propertyList.Add("beverly-hills/5-star-luxury-hotel-beverly-hills");
           // propertyList.Add("bangkok/5-star-luxury-hotel-riverside");
           // propertyList.Add("manila/5-star-luxury-hotel-makati");


            foreach(var language in languageList)
            {
                foreach (var property in propertyList)
                {
                    var url = $"{staging}/{language}/{property}";
                    Console.WriteLine($"url:{url}");
                    using (var httpClient = new HttpClient())
                    {
                        
                        var response = httpClient.GetAsync(url);
                        if (response.Result.StatusCode == HttpStatusCode.Redirect)
                        {
                            continue;
                        }
                    }
                    //1 go to property home page
                    driver.Url = url;
                    
                    var element = driver.FindElement(By.XPath("//div[@data-react-component='CampaignReservationsWText']"));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);


                    var button = driver.FindElement(By.XPath("//div[@data-react-component='CampaignReservationsWText']//button[@type='button']"));
                    button.Click();

                    var button20 = driver.FindElements(By.XPath("//button[@type='button' and @class='CalendarDay_button CalendarDay_button_1' and text()='2']"));
                    button20[0].Click();

                   // var imagenext = driver.FindElement(By.XPath("//div[@data-react-component='ImageGallery']//button[@class='icon-arrow-right Right']"));
                   // imagenext.Click();

                   // var buttonnext = driver.FindElement(By.XPath("//div[@data-react-component='CampaignReservationsWText']//input[@type='text' and @name='endDate']"));
                   // buttonnext.Click();

                    var button21 = driver.FindElements(By.XPath("//button[@type='button' and @class='CalendarDay_button CalendarDay_button_1' and text()='3']"));
                    button21[0].Click();

                    driver.FindElement(By.XPath("//div[@data-react-component='CampaignReservationsWText']//button[@type='submit']")).Click();
                    return;
                    var text = driver.FindElement(By.XPath("//input[@id='startDate']"));
                    var className = text.GetAttribute("class");
                    var aria = text.GetAttribute("aria-label");
                    var name = text.GetAttribute("name");
                    text.SendKeys(Keys.Enter);
                    // driver.FindElement(By.XPath("//input[@id='startDate']")).Click();//.SendKeys(Keys.Enter);
                    return;

                    driver.FindElement(By.Id("focus-3")).Click();


                    var offerList = driver.FindElements(By.XPath("//div[@class='cardMedium-cta']/a"));


                    foreach (IWebElement clickElement in offerList)
                    {
                        var href = clickElement.GetAttribute("href");
                        if (href.IndexOf("/room") > 0)
                        {
                            Console.WriteLine(href);
                        }
                    }

                    return;


                    //2 go to room list page
                    // driver.FindElement(By.XPath("//*[@id='focus-0']")).Click();
                    driver.FindElement(By.Id("focus-0")).Click();
                    // 3 find all room







                    var roomList = driver.FindElements(By.XPath("//div[@class='cardRoomDetails-link']/a"));


                    foreach (IWebElement clickElement in roomList)
                    {
                        //var href = clickElement.GetAttribute("href");
                       // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                       // executor.ExecuteScript("window.open('" + href + "')");
                    }

                    var roomCheckAvailabilityList = driver.FindElements(By.XPath("//div[@class='cardRoomDetails-cta']/a"));

                    foreach (IWebElement clickElement in roomCheckAvailabilityList)
                    {
                        var href = clickElement.GetAttribute("href");
                        Console.WriteLine(href);
                        using (var httpClient = new HttpClient())
                        {
                            var response = httpClient.GetAsync(url);
                            if (response.Result.StatusCode == HttpStatusCode.Redirect)
                            {
                                Console.WriteLine(response.Result.Headers.Location.AbsolutePath);
                            }
                        }
                    }

                }
            }
        }

        public static void Test()
        {
            var url = string.Format("https://www.baidu.com/");
            IWebDriver driver;
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            driver = new ChromeDriver(@"C:\Program Files (x86)\Google\Chrome\Application", options);
            driver.Url = url;

            var list = new List<string>();
            list.Add("http://malts.diageoplatform.com/en-gb/visit-a-distillery?agp=true&type=redirect");
            list.Add("http://malts.diageoplatform.com/en-au/home?agp=true");
            list.Add("http://malts.diageoplatform.com/en-row/home?agp=true");
            list.Add("http://malts.diageoplatform.com/en-us/home?agp=true");
            list.Add("http://malts.diageoplatform.com/en-gb/home?agp=true");
            list.Add("http://malts.diageoplatform.com/en-row/whisky-guide/whisky-types/?agp=true");
            list.Add("http://malts.diageoplatform.com/en-row/our-whisky-collection/?agp=true");
            list.Add("http://malts.diageoplatform.com/en-row/visit-a-distillery/glen-ord?agp=true");
            list.Add("http://malts.diageoplatform.com/en-row/find-a-whisky-for-you?agp=true");
            list.Add("http://malts.diageoplatform.com/en-row/whisky-guide/glen-ord?agp=true");

            // var list = driver.FindElements(By.XPath("//*[@id='u1']/a"));

            foreach (var r in list)
            {
                //webElement.Click();
                //String href = webElement.GetAttribute("href");
                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                executor.ExecuteScript("window.open('" + r + "')");
            }

            //Thread.Sleep(10000);
            //var curHandle = driver.CurrentWindowHandle;
            //foreach(var handle in driver.WindowHandles)
            //{
            //    if(handle.Equals(curHandle))
            //    {
            //        continue;
            //    }
            //    driver.SwitchTo().Window(handle);
            //    driver.Close();
            //}
    
         
        }


        public static void BuildScript()
        {
            var result = new List<RestaurantScriptModel>();

            var str = File.ReadAllText(@"C:\Document\wagamama\sprint3\opentime.json");

            var restaurantList = JsonConvert.DeserializeObject<List<RestaurantOpenModel>>(str);
           
            foreach (var r in restaurantList)
            {
                BuildDataList(r.Open20191224, r.Close20191224, "2019-12-24", r.GUID, result);
                BuildDataList(r.Open20191225, r.Close20191225, "2019-12-25", r.GUID, result);
                BuildDataList(r.Open20191226, r.Close20191226, "2019-12-26", r.GUID, result);
                BuildDataList(r.Open20191227, r.Close20191227, "2019-12-27", r.GUID, result);
                BuildDataList(r.Open20191228, r.Close20191228, "2019-12-28", r.GUID, result);
                BuildDataList(r.Open20191229, r.Close20191229, "2019-12-29", r.GUID, result);
                BuildDataList(r.Open20191230, r.Close20191230, "2019-12-30", r.GUID, result);
                BuildDataList(r.Open20191231, r.Close20191231, "2019-12-31", r.GUID, result);
                BuildDataList(r.Open20200101, r.Close20200101, "2020-01-01", r.GUID, result);
            }

            StringBuilder sb = new StringBuilder();

            foreach(var r in restaurantList)
            {
                var list = result.Where(x => x.Guid == r.GUID);

                foreach(var d in list)
                {
                    sb.AppendLine($"Insert into SpecialAvailability (RestaurantID,Date,TimeSlotID,Available) values ('{d.Guid}','{d.Date}',{d.TimeId},{d.Available});"); 

                }
                sb.AppendLine();
                sb.AppendLine();

            }

            File.WriteAllText(@"C:\Document\wagamama\sprint3\script.txt", sb.ToString());

        }

        public static void BuildDataList(string open, string close, string date, string guid, List<RestaurantScriptModel> result)
        {
            var timeList = Timeslot.TimeList();
            if (open.Trim().ToLower().Equals("closed"))
            {
                foreach (var time in timeList)
                {
                    var data = BuildData(guid, date, time.Id, 0);
                    result.Add(data);
                }

            }
            else
            {
                var startIndex = timeList.FirstOrDefault(x => x.Time.Trim() == open.Trim())?.Id;
                var endIndex = timeList.FirstOrDefault(x => x.Time.Trim() == close.Trim())?.Id;
                if (startIndex != null && startIndex > 0 && endIndex != null && endIndex > 0)
                {
                    foreach (var time in timeList)
                    {
                        int available = 1;
                        if (time.Id < startIndex || endIndex < time.Id)
                        {
                            available = 0;
                        }
                        var data = BuildData(guid, date, time.Id, available);
                        result.Add(data);
                    }
                }
            }
        }



        public static RestaurantScriptModel BuildData(string guid,string date,int timeId,int available)
        {
            var data = new RestaurantScriptModel();
            data.Guid = guid;
            data.Date = date;
            data.TimeId = timeId;
            data.Available = available;
            return data;
        }


    }

    public class RestaurantScriptModel
    {
        public string Guid { get; set; }
        public string Date { get; set; }
        public int TimeId { get; set; }
        public int Available { get; set; }
    }

    public class RestaurantOpenModel
    {
        [JsonProperty("Guid")]
        public string GUID { get; set; }

        [JsonProperty("2019-12-24-open")]
        public string Open20191224 { get; set; }

        [JsonProperty("2019-12-24-close")]
        public string Close20191224 { get; set; }

        [JsonProperty("2019-12-25-open")]
        public string Open20191225 { get; set; }

        [JsonProperty("2019-12-25-close")]
        public string Close20191225 { get; set; }

        [JsonProperty("2019-12-26-open")]
        public string Open20191226 { get; set; }

        [JsonProperty("2019-12-26-close")]
        public string Close20191226 { get; set; }

        [JsonProperty("2019-12-27-open")]
        public string Open20191227 { get; set; }

        [JsonProperty("2019-12-27-close")]
        public string Close20191227 { get; set; }

        [JsonProperty("2019-12-28-open")]
        public string Open20191228 { get; set; }

        [JsonProperty("2019-12-28-close")]
        public string Close20191228 { get; set; }

        [JsonProperty("2019-12-29-open")]
        public string Open20191229 { get; set; }

        [JsonProperty("2019-12-29-close")]
        public string Close20191229 { get; set; }

        [JsonProperty("2019-12-30-open")]
        public string Open20191230 { get; set; }

        [JsonProperty("2019-12-30-close")]
        public string Close20191230 { get; set; }

        [JsonProperty("2019-12-31-open")]
        public string Open20191231 { get; set; }

        [JsonProperty("2019-12-31-close")]
        public string Close20191231 { get; set; }

        [JsonProperty("2020-01-01-open")]
        public string Open20200101 { get; set; }

        [JsonProperty("2020-01-01-close")]
        public string Close20200101 { get; set; }
    }

    public class TimeSlotsModel
    {
        public int Id { get; set; }
        public string Time { get; set; }
    }
    public static class Timeslot
    {
        public static List<TimeSlotsModel> TimeList()
        {
            var list = new List<TimeSlotsModel>();

            list.Add(new TimeSlotsModel { Id = 9, Time = "4:00" });
            list.Add(new TimeSlotsModel { Id = 10, Time = "4:30" });
            list.Add(new TimeSlotsModel { Id = 11, Time = "5:00" });
            list.Add(new TimeSlotsModel { Id = 12, Time = "5:30" });
            list.Add(new TimeSlotsModel { Id = 13, Time = "6:00" });
            list.Add(new TimeSlotsModel { Id = 14, Time = "6:30" });
            list.Add(new TimeSlotsModel { Id = 15, Time = "7:00" });
            list.Add(new TimeSlotsModel { Id = 16, Time = "7:30" });
            list.Add(new TimeSlotsModel { Id = 17, Time = "8:00" });
            list.Add(new TimeSlotsModel { Id = 18, Time = "8:30" });
            list.Add(new TimeSlotsModel { Id = 19, Time = "9:00" });
            list.Add(new TimeSlotsModel { Id = 20, Time = "9:30" });
            list.Add(new TimeSlotsModel { Id = 21, Time = "10:00" });
            list.Add(new TimeSlotsModel { Id = 22, Time = "10:30" });
            list.Add(new TimeSlotsModel { Id = 23, Time = "11:00" });
            list.Add(new TimeSlotsModel { Id = 24, Time = "11:30" });
            list.Add(new TimeSlotsModel { Id = 25, Time = "12:00" });
            list.Add(new TimeSlotsModel { Id = 26, Time = "12:30" });
            list.Add(new TimeSlotsModel { Id = 27, Time = "13:00" });
            list.Add(new TimeSlotsModel { Id = 28, Time = "13:30" });
            list.Add(new TimeSlotsModel { Id = 29, Time = "14:00" });
            list.Add(new TimeSlotsModel { Id = 30, Time = "14:30" });
            list.Add(new TimeSlotsModel { Id = 31, Time = "15:00" });
            list.Add(new TimeSlotsModel { Id = 32, Time = "15:30" });
            list.Add(new TimeSlotsModel { Id = 33, Time = "16:00" });
            list.Add(new TimeSlotsModel { Id = 34, Time = "16:30" });
            list.Add(new TimeSlotsModel { Id = 35, Time = "17:00" });
            list.Add(new TimeSlotsModel { Id = 36, Time = "17:30" });
            list.Add(new TimeSlotsModel { Id = 37, Time = "18:00" });
            list.Add(new TimeSlotsModel { Id = 38, Time = "18:30" });
            list.Add(new TimeSlotsModel { Id = 39, Time = "19:00" });
            list.Add(new TimeSlotsModel { Id = 40, Time = "19:30" });
            list.Add(new TimeSlotsModel { Id = 41, Time = "20:00" });
            list.Add(new TimeSlotsModel { Id = 42, Time = "20:30" });
            list.Add(new TimeSlotsModel { Id = 43, Time = "21:00" });
            list.Add(new TimeSlotsModel { Id = 44, Time = "21:30" });
            list.Add(new TimeSlotsModel { Id = 45, Time = "22:00" });
            list.Add(new TimeSlotsModel { Id = 46, Time = "22:30" });
            list.Add(new TimeSlotsModel { Id = 47, Time = "23:00" });
            list.Add(new TimeSlotsModel { Id = 48, Time = "23:30" });

            return list;

        }
    }
}
