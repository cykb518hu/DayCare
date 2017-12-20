using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DayCare
{
    class ScapeSanData
    {

        public static void MainFunc()
        {
           
            var typeList = new List<DayCareType>();
            typeList.Add(new DayCareType { Id = 830, Type = "Child Care - Infant Center" });
            typeList.Add(new DayCareType { Id = 840, Type = "School Age Child Care Center" });
            typeList.Add(new DayCareType { Id = 845, Type = "Child Care Center" });
            typeList.Add(new DayCareType { Id = 850, Type = "Child Care Center Preschool" });

            var zipCodeList = @"94127,94133,94132,94133,94134,94102,94158,94158,94158,94103,94104,94105,94107,94108,94109,94110,94111,94112,94114,94115,94116,94117,94118,94121,94121,94122,94130";
            foreach(var r in typeList)
            {
                var list = new List<FACILITYARRAY>();
                var url = "";
                if (r.Id == 850)
                {
                    var arr = zipCodeList.Split(',');
                    foreach(var z in arr)
                    {
                        url = string.Format("https://secure.dss.ca.gov/ccld/TransparencyAPI/api/FacilitySearch?facType={0}&facility=&Street=&city=&zip={1}&county=San%20Francisco&facnum=", r.Id, z);
                       var result= Getdata(url);
                        if(result!=null&&result.FACILITYARRAY.Any())
                        {
                            list.AddRange(result.FACILITYARRAY);
                        }
                        
                    }
                    LocalExcel.CreateLocalExcelForOnece(list, r.Type);
                }
                else
                {
                    url = string.Format("https://secure.dss.ca.gov/ccld/TransparencyAPI/api/FacilitySearch?facType={0}&facility=&Street=&city=&zip=&county=San%20Francisco&facnum=", r.Id);
                    var result = Getdata(url);
                    if (result != null && result.FACILITYARRAY.Any())
                    {
                        list.AddRange(result.FACILITYARRAY);
                    }
                    LocalExcel.CreateLocalExcelForOnece(list, r.Type);
                }
            }
        }

        public static RootObject Getdata(string url)
        {
            var model = new RootObject();
            try
            {
                using (var client = new HttpClient())
                {
                    var stringTask = client.GetStringAsync(url).Result;
                    var result = JObject.Parse(stringTask);
                    model = JsonConvert.DeserializeObject<RootObject>(stringTask);
                }
            }
            catch (Exception ex)
            {

            }
            return model;
        }

    }

    public class DayCareType
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("COUNT")]
        public int COUNT { get; set; }
        [JsonProperty("FACILITYARRAY")]
        public List<FACILITYARRAY> FACILITYARRAY { get; set; }
    }
    public class FACILITYARRAY
    {
        [JsonProperty("COUNTY")]
        public string COUNTY { get; set; }
        [JsonProperty("FACILITYNAME")]
        public string FACILITYNAME { get; set; }
        [JsonProperty("FACILITYNUMBER")]
        public string FACILITYNUMBER { get; set; }
        [JsonProperty("STATUS")]
        public string STATUS { get; set; }
        [JsonProperty("STREETADDRESS")]
        public string STREETADDRESS { get; set; }
        [JsonProperty("TELEPHONE")]
        public string TELEPHONE { get; set; }
        [JsonProperty("ZIPCODE")]
        public string ZIPCODE { get; set; }
    }

}
