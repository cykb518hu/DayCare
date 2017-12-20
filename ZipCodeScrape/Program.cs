
using DayCareDataModel;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
            Municiplity();
            return;
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

        static void Municiplity()
        {
            List<MuniciplityCounty> list = new List<MuniciplityCounty>();
            Dictionary<string, string> cityScrapeList = new Dictionary<string, string>();
            string connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ToString();

                string queryString = @"select * from city_temp";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!cityScrapeList.ContainsKey(reader["city_nm"].ToString()))
                    {
                        cityScrapeList.Add(reader["city_nm"].ToString(), reader["deploye_date"].ToString());
                    }
                }
            }

            

            Dictionary<string, string> wikeList = new Dictionary<string, string>();
            var url = string.Format("https://en.wikipedia.org/wiki/List_of_municipalities_in_Michigan_(by_population)");
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection dayCareCenterNodes = doc.DocumentNode.SelectNodes("//table[@class='wikitable sortable']");
            foreach (HtmlNode table in dayCareCenterNodes)
            {
                var rows = table.SelectNodes("tr");
                foreach (var r in rows)
                {
                    if (r.SelectNodes("td") != null)
                    {
                        var municiplity = r.SelectNodes("td")[1].FirstChild.InnerText;
                        var county = r.SelectNodes("td")[2].FirstChild.InnerText;
                        wikeList.Add(municiplity, county);
                    }
                }
            }
            foreach (var r in cityScrapeList)
            {
                var data = new MuniciplityCounty();
                data.Municiplity = r.Key;
                data.DeployDate = r.Value;
                foreach (var w in wikeList)
                {
                    if (r.Key.IndexOf("MICity") > 0)
                    {
                        var orignmuni = r.Key.Replace("MICity", "").ToLower();
                        var wmuni = w.Key.Replace(" ", string.Empty).ToLower();
                        if(wmuni.IndexOf("(")>0)
                        {
                            continue;
                           // wmuni = wmuni.Substring(0, wmuni.IndexOf("("));
                        }
                        if (wmuni.Contains(orignmuni))
                        {
                            data.Typ = "City";
                            data.ShortNm = w.Key;
                            data.County = w.Value;
                            data.LongNm = "City of " + w.Key;
                            continue;
                        }

                    }


                    if (r.Key.IndexOf("TownshipMI") > 0)
                    {
                        var orignmuni = r.Key.Replace("TownshipMI", "").Replace("Charter", "").ToLower();
                        var wmuni = w.Key.Replace(" ", string.Empty).ToLower();
                        if (wmuni.IndexOf("(") > 0)
                        {
                            if (wmuni.Contains(orignmuni))
                            {
                                data.Typ = "Township";
                                data.ShortNm = w.Key.Substring(0, w.Key.IndexOf("(")).Trim();
                                data.County = w.Value;
                                data.LongNm = data.ShortNm + " Township";
                                continue;
                            }
                        }
                    }



                }
                list.Add(data);
            }
            connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ToString();
            foreach (var rr in list)
            {
                 queryString = @"INSERT INTO CITY (DEPLOYE_DATE,SHORT_NM,COUNTY_NM,CITY_NM,typ,long_nm) VALUES('" + rr.DeployDate + "','" + rr.ShortNm + "','" + rr.County + "','" + rr.Municiplity + "','" + rr.Typ + "','" + rr.LongNm + "')";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }

            }
        }
    }

    public class MuniciplityCounty
    {
        public string Municiplity { get; set; }
        public string ShortNm { get; set; }
        public string County { get; set; }
        public string DeployDate { get; set; }

        public string Typ { get; set; }
        public string LongNm { get; set; }
    }


}
