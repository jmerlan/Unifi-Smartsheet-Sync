using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ContentManagement
{
    public class Diff
    {
        public string Key { get; set; }
        public string AssetValue { get; set; }
        public string FamilyValue { get; set; }

        public static Diff CompareParameters(Asset asset, Entities.Content content)
        {
            if (asset.Manufacturer == content.Manufacturer)
            {
                // Return null because the Content is synced
                return null;
            }
            else
            {
                // Pass the values to a Diff object and return
                var diff = new Diff();

                diff.Key = asset.Id;
                diff.AssetValue = asset.Manufacturer;
                diff.FamilyValue = content.Manufacturer;

                return diff;
            }
        }
    }
}
