using Newtonsoft.Json;
using RestSharp;
using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace ContentManagement
{
    public class Asset
    {
        public string State { get; set; }
        public string Description { get; set; }
        public bool? Merchandisable { get; set; }
        public string Id { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Vendor { get; set; }
        public string ProcuredBy { get; set; }
        public string ReceivedBy { get; set; }
        public string InstalledBy { get; set; }
        public string ConnectBy { get; set; }
        public bool IsSynced { get; set; }
        public string SyncComments { get; set; }
        public string Department { get; set; }
        public string[] SubDepartment { get; set; }
        public double CapMultiplier { get; set; }

        /// <summary>
        /// Casts a Row from Smarthsheet to an Asset
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Asset FromRow(Row row)
        {
            var asset = new Asset();

            // Assign properties based on Smartsheet cell values
            asset.State =   Connector.GetCellValue(row, 7276296175675268);
            asset.Description = Connector.GetCellValue(row, 4571044452296580);
            asset.Id = Connector.GetCellValue(row, 4590715224254340);
            asset.Manufacturer = Connector.GetCellValue(row, 8280756402317188);
            asset.Model = Connector.GetCellValue(row, 5133994405717892);
            asset.Vendor = Connector.GetCellValue(row, 630394778347396);
            asset.ProcuredBy = Connector.GetCellValue(row, 8162260536321924);
            asset.ReceivedBy = Connector.GetCellValue(row, 1858222396073860);
            asset.InstalledBy = Connector.GetCellValue(row, 6361822023444356);
            asset.ConnectBy = Connector.GetCellValue(row, 4110022209759108);
            asset.Department = Connector.GetCellValue(row, 6096515000231812);
            asset.Merchandisable = IsMerchandisable(row);
            
            // Do work to get the multiple subdepartments as a list
            var subDeptString = Connector.GetCellValue(row, 3844715186546564);
            if (!string.IsNullOrEmpty(subDeptString))
            { 
                if (subDeptString.Contains(','))
                {
                    asset.SubDepartment = Array.ConvertAll(subDeptString.Split(','), p => p.Trim());
                }
                // If there is only one subdepartment, get the string as a single list item
                else
                {
                    asset.SubDepartment = new string[] { subDeptString };
                }
            }

            // Do work to get the capacity multiplier as a number
            string capMultiplierString = Connector.GetCellValue(row, 512218688186244);
            if (!string.IsNullOrEmpty(capMultiplierString))
            {
                try
                {
                    asset.CapMultiplier = Convert.ToInt32(capMultiplierString);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                asset.CapMultiplier = 1;
            }

            if (asset.CapMultiplier == 0)
            {
                asset.CapMultiplier = 1;
            }

            return asset;
        }

        /// <summary>
        /// Checks if an Asset has customer interaction
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static bool? IsMerchandisable (Row row)
        {
            // Null until proven otherwise
            bool? hasCx = null;

            // Checkbox from Smartsheet returns a string. Cast this to a bool.
            var smartsheetValue = Connector.GetCellValue(row, 3890439355950980);
            if (smartsheetValue == "True")
            {
                hasCx = true;
            }
            if (smartsheetValue == "False")
            {
                hasCx = false;
            }

            return hasCx;
        }
    }
}
