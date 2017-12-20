using DayCareDataModel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DayCare
{
    class GoogleSheetApi
    {
        public SheetsService AuthorizeGoogleApp()
        {
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string ApplicationName = "Google Sheets API .NET Quickstart";
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "achilles@roundcubegroup.com",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            return service;
        }

        public void CreateNewSheet(List<DayCareModel> list)
        {
            foreach(var r in list.GroupBy(x=>x.FacilityInformation.County))
            {
                var subList = list.Where(x => x.FacilityInformation.County.Equals(r.Key)).ToList();
                var workSheetName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                // var spreadsheetId = ConfigurationManager.AppSettings[r.Key.ToUpper()];// "1gXw3lPDojKWwtzRVLwSkBD0vUzJtoWnKVbY5cbHvaOg";
                var spreadsheetId = "1kVeW2pbKhu1NvN8UpT6FpG05dpHxzuuJAi2X9Sd0n9A";// ConfigurationManager.AppSettings["1QF3BsrGgbouHbnNisvc6vs8LYzVIp62QU7G1tEzClcE"];
                InitialSheet(workSheetName, spreadsheetId);
                UpdateSheet(subList, workSheetName, spreadsheetId);
            }
            
        }


        public void InitialSheet(string workSheetName, string spreadsheetId)
        {
            var service = AuthorizeGoogleApp();
            var addSheetRequest = new AddSheetRequest();
            addSheetRequest.Properties = new SheetProperties();
            addSheetRequest.Properties.Title = workSheetName;
            addSheetRequest.Properties.Index = 0;

            BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = new List<Request>();

            batchUpdateSpreadsheetRequest.Requests.Add(new Request
            {
                AddSheet = addSheetRequest
         
            });
            var batchUpdateRequest =
                service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadsheetId);

            batchUpdateRequest.Execute();
        }

        public void UpdateSheet(List<DayCareModel> list, string workSheetName, string spreadsheetId)
        {
            var service = AuthorizeGoogleApp();

            String range = string.Format("{0}!{1}", workSheetName, "A:AA"); //"工作表1!A1:E";
            ValueRange valueRange = new ValueRange();
            valueRange.Range = range;
            valueRange.MajorDimension = "ROWS";//"ROWS";//COLUMNS
            var oblist = new List<object>() { "Status", "Facility Name" , "Facility Street" , "Facility City" , "Facility State" , "Facility Zip" , "Facility County" , "Facility Phone" , "Facility LicenseStatus" ,
                "Licensee Name" , "Licensee Address", "Licensee Phone",
                "License Number","License FacilityType","License Capacity","License EffectiveDate","License ExpirationDate","License PeriodOfOperation",
                "DaysOpen Sunday","DaysOpen Monday","DaysOpen Tuesday","DaysOpen Wednesday","DaysOpen Thursday","DaysOpen Friday","DaysOpen Saturday",
                "ServicesOffered FullDay", "ServicesOffered Provides" };
            var dataList = HandleData(list);
            valueRange.Values = new List<IList<object>> { oblist };
            foreach (var r in dataList)
            {
                valueRange.Values.Add(r);
            }

            SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            UpdateValuesResponse result2 = update.Execute();
        }

        public List<IList<object>> HandleData(List<DayCareModel> list)
        {
            var data = new List<IList<object>>();

            foreach(var r in list)
            {
                var oblist = new List<object>() { r.FacilityInformation.Status, r.FacilityInformation.Name,  r.FacilityInformation.Street ,  r.FacilityInformation.City ,  r.FacilityInformation.State, r.FacilityInformation.ZipCode, r.FacilityInformation.County , r.FacilityInformation.Phone, r.FacilityInformation.LicenseStatus,
                    r.LicenseeInformation.Name , r.LicenseeInformation.Address, r.LicenseeInformation.Phone ,
                    r.LicenseInformation.Number,r.LicenseInformation.FacilityType,r.LicenseInformation.Capacity,r.LicenseInformation.EffectiveDate,r.LicenseInformation.ExpirationDate,r.LicenseInformation.PeriodOfOperation,
                    r.DaysOpen.Sunday,r.DaysOpen.Monday,r.DaysOpen.Tuesday,r.DaysOpen.Wednesday,r.DaysOpen.Thursday,r.DaysOpen.Friday,r.DaysOpen.Saturday,
                    r.ServicesOffered.FullDayProgram,r.ServicesOffered.Provides};
                data.Add(oblist);
            }

            return data;
        }
        

    }
}
