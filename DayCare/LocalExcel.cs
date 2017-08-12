using DayCareDataModel;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCare
{
    public class LocalExcel
    {
        public void CreateLocalExcel(List<DayCareModel> list)
        {

            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            xlApp.Visible = true;
            Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = (Worksheet)wb.Worksheets[1];

            ws.Cells[1, 2] = "Facility Name";
            ws.Cells[1, 3] = "Facility Street";
            ws.Cells[1, 4] = "Facility City";
            ws.Cells[1, 5] = "Facility State";
            ws.Cells[1, 6] = "Facility Zip";
            ws.Cells[1, 7] = "Facility County";
            ws.Cells[1, 8] = "Facility Phone";
            ws.Cells[1, 9] = "Facility LicenseStatus";

            ws.Cells[1, 10] = "Licensee Name";
            ws.Cells[1, 11] = "Licensee Address";
            ws.Cells[1, 12] = "Licensee Phone";

            ws.Cells[1, 13] = "License Number";
            ws.Cells[1, 14] = "License FacilityType";
            ws.Cells[1, 15] = "License Capacity";
            ws.Cells[1, 16] = "License EffectiveDate";
            ws.Cells[1, 17] = "License ExpirationDate";
            ws.Cells[1, 18] = "License PeriodOfOperation";

            ws.Cells[1, 19] = "DaysOpen Sunday";
            ws.Cells[1, 20] = "DaysOpen Monday";
            ws.Cells[1, 21] = "DaysOpen Tuesday";
            ws.Cells[1, 22] = "DaysOpen Wednesday";
            ws.Cells[1, 23] = "DaysOpen Thursday";
            ws.Cells[1, 24] = "DaysOpen Friday";
            ws.Cells[1, 25] = "DaysOpen Saturday";

            ws.Cells[1, 26] = "ServicesOffered FullDayProgram";
            ws.Cells[1, 27] = "ServicesOffered Provides";

            int row = 2;
            foreach (var r in list)
            {
                ws.Cells[row, 2] = r.FacilityInformation.Name;
                ws.Cells[row, 3] = r.FacilityInformation.Street;
                ws.Cells[row, 4] = r.FacilityInformation.City;
                ws.Cells[row, 5] = r.FacilityInformation.State;
                ws.Cells[row, 6] = r.FacilityInformation.ZipCode;
                ws.Cells[row, 7] = r.FacilityInformation.County;
                ws.Cells[row, 8] = r.FacilityInformation.Phone;
                ws.Cells[row, 9] = r.FacilityInformation.LicenseStatus;

                ws.Cells[row, 10] = r.LicenseeInformation.Name;
                ws.Cells[row, 11] = r.LicenseeInformation.Address;
                ws.Cells[row, 12] = r.LicenseeInformation.Phone;


                ws.Cells[row, 13] = r.LicenseInformation.Number;
                ws.Cells[row, 14] = r.LicenseInformation.FacilityType;
                ws.Cells[row, 15] = r.LicenseInformation.Capacity;
                ws.Cells[row, 16] = r.LicenseInformation.EffectiveDate;
                ws.Cells[row, 17] = r.LicenseInformation.ExpirationDate;
                ws.Cells[row, 18] = r.LicenseInformation.PeriodOfOperation;

                ws.Cells[row, 19] = r.DaysOpen.Sunday;
                ws.Cells[row, 20] = r.DaysOpen.Monday;
                ws.Cells[row, 21] = r.DaysOpen.Tuesday;
                ws.Cells[row, 22] = r.DaysOpen.Wednesday;
                ws.Cells[row, 23] = r.DaysOpen.Thursday;
                ws.Cells[row, 24] = r.DaysOpen.Friday;
                ws.Cells[row, 25] = r.DaysOpen.Saturday;

                ws.Cells[row, 26] = r.ServicesOffered.FullDayProgram;
                ws.Cells[row, 27] = r.ServicesOffered.Provides;

                row++;
            }
            var file = ConfigurationManager.AppSettings.Get("LocalExcelPath").ToString();
            file += DateTime.Now.ToString("yyyy-MM-dd") + Guid.NewGuid().ToString() + ".xlsx";
            wb.SaveAs(file, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
        false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
        Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        }
    }
}
