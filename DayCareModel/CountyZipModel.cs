using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareDataModel
{
    public class ScrapeSource
    {
        public string County { get; set; }
        public string ZipCode { get; set; }
        public string DetailUrl { get; set; }
    }
    public class CountyZipModel
    {
        public string CountyCode { get; set; }
        public string County { get; set; }
        public List<ZipCodeModel> ZipCodeList { get; set; }
        public bool UseCounty { get; set; }
    }
    public class ZipCodeModel
    {
        public string ZipCode { get; set; }
    }

    public class DayCareModel
    {
        public FacilityInfo FacilityInformation { get; set; }
        public LicenseeInfo LicenseeInformation { get; set; }
        public LicenseInfo LicenseInformation { get; set; }
        public DaysOpen DaysOpen { get; set; }
        public ServicesOffered ServicesOffered { get; set; }
    }
    public class FacilityInfo
    {
        public string Status { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string County { get; set; }
        public string Phone { get; set; }
        public string LicenseStatus { get; set; }
        public int ZipOrder { get; set; }
    }

    public class LicenseeInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }

    public class LicenseInfo
    {
        public string Number { get; set; }
        public string FacilityType { get; set; }
        public string Capacity { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }
        public string PeriodOfOperation { get; set; }
    }
    public class DaysOpen
    {
        public string Sunday { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Saturday { get; set; }
    }
    public class ServicesOffered
    {
        public string FullDayProgram { get; set; }
        public string Provides { get; set; }
    }
}
