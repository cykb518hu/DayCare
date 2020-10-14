using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DayCare
{
    public static class ReadDisease
    {
        public static void GetData()
        {


            var hpoStr = File.ReadAllText(@"C:\Users\huzhe\Source\Repos\DayCare\DayCare\TextFile2.txt", Encoding.UTF8);
            var list = new List<HPODataModel>();
            hpoStr = hpoStr.Replace(" ", "").Replace("'", "");
            hpoStr = hpoStr.Substring(2, hpoStr.Length - 2);
            hpoStr = hpoStr.Substring(0, hpoStr.Length - 2);
            string[] hpoArray = hpoStr.Split(new string[] { "],[" }, StringSplitOptions.None);
            var regex = new Regex("HP:");
            foreach (var s in hpoArray)
            {
                var single = s;
                single = single.Replace("[", "").Replace("]", "");
                var index = regex.Matches(single).Count;
                int startIndex = 0; 
                if(list.LastOrDefault()!=null)
                {
                    startIndex = list.LastOrDefault().StartIndex;
                }
                for (int i = 0; i < index; i++)
                {
                    var data = new HPODataModel();
                    string[] subArrary = single.Split(',');
                    var currentStartIndex = Convert.ToInt32(subArrary[0]);
                    var currentEndIndex = Convert.ToInt32(subArrary[1]);
                    var length = currentEndIndex - currentStartIndex;
                    data.StartIndex = currentStartIndex + startIndex;
                    data.EndIndex = data.StartIndex + length;
                    data.Name = subArrary[2];
                    data.HpoId = subArrary[3 + i];
                    data.IndexList = new List<HPOMatchIndexModel>();
                    data.IndexList.Add(new HPOMatchIndexModel { StartIndex = data.StartIndex, EndIndex = data.EndIndex });
                    data.Count = data.IndexList.Count;
                    var item = list.FirstOrDefault(x => x.HpoId == data.HpoId);
                    if(item!=null)
                    {
                        item.IndexList.AddRange(data.IndexList);
                        item.Count = item.IndexList.Count;
                    }
                    else
                    {
                        list.Add(data);
                    } 
                }           
            }
            return;
            // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册简体中文的支持

            var cityStr = File.ReadAllText(@"C:\Users\huzhe\Source\Repos\DayCare\DayCare\city  group byjson.json", Encoding.UTF8);
            //cityStr = cityStr.Replace("\r\n", "");

            var citydata = JsonConvert.DeserializeObject<List<PatientCity>>(cityStr);

            var allStr = File.ReadAllText(@"C:\Users\huzhe\Source\Repos\DayCare\DayCare\all data.json", Encoding.UTF8);

            var alldata = JsonConvert.DeserializeObject<List<AllDataSheng>>(allStr);

            foreach(var r in citydata)
            {
                var data = alldata.FirstOrDefault(x => x.areaList.Any(y => r.Pers_BasicInfo_BirthPlaceCityName.Trim().Contains(y.name)));
                if(data!=null)
                {
                    r.Sheng = data.name;
                }
                else
                {
                    data = alldata.FirstOrDefault(x => x.areaList.Any(y => y.areaList.Any(z => r.Pers_BasicInfo_BirthPlaceCityName.Trim().Contains(z.name))));
                    if(data!=null)
                    {
                        r.Sheng = data.name;
                    }
                }
            }
            var last =new List<SeriesDataModel>();
            foreach(var r in citydata.GroupBy(x=>x.Sheng))
            {
                var data = new SeriesDataModel();
                data.Name = r.Key;
                data.Value = r.Sum(x => x.Number);
                last.Add(data);
            }
            var result = JsonConvert.SerializeObject(last).ToString();
            return;
            List<DiseasesHref> diseaseList = new List<DiseasesHref>();
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load("http://localhost:8002/e2113203d0bf45d181168d855426ca7c.html");
            HtmlNodeCollection dayCareCenterNodes = doc.DocumentNode.SelectNodes("//div[@id='outline']/ul/li/a");

            foreach (HtmlNode node in dayCareCenterNodes)
            {
                var data = new DiseasesHref();
                data.Name = node.InnerText;
                data.Href = node.Attributes["href"].Value;
                diseaseList.Add(data);
            }

            var str = JsonConvert.SerializeObject(diseaseList);

        }
    }
    public class Root
    {
        public List<List<object>> MyArray { get; set; }
    }

    public class HPODataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameEnglish")]
        public string NameEnglish { get; set; }

        [JsonProperty("hpoId")]
        public string HpoId { get; set; }

        [JsonProperty("certain")]
        public string Certain { get; set; }

        [JsonProperty("isSelf")]
        public string IsSelf { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }

        [JsonProperty("editable")]
        public bool Editable { get; set; }

        [JsonProperty("matched")]
        public string Matched { get; set; }


        [JsonProperty("hasExam")]
        public bool HasExam { get; set; }

        [JsonProperty("indexList")]
        public List<HPOMatchIndexModel> IndexList { get; set; }


    }
    public class HPOMatchIndexModel
    {
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
    }
    public class DiseasesHref
    {
        public string Name { get; set; }
        public string Href { get; set; }
    }

    public class SeriesDataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class PatientCity
    {
        public string total { get; set; }

        public int Number { get { return Convert.ToInt32(total); } }

        public string Sheng { get; set; }

        public string Pers_BasicInfo_BirthPlaceCityName { get; set; }
    }

    public class AllDataSheng
    {
        public string code { get; set; }
        public string name { get; set; }
        public List<AllDataSheng> areaList { get; set; }
    }
}
