using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagement
{
    public class Manager
    {
        public List<Asset> Assets { get; set; }
        public List<Entities.Content> Families { get; set; }
        public List<Entities.Library> Libraries { get; set; }

        public static void CompareParameters(Asset asset, Entities.Content content, Entities.Parameter parameter)
        {

        }
    }
}
