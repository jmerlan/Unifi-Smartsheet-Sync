using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ContentManagement
{
    public class Manager
    {
        public List<Asset> Assets { get; set; }
        public List<Entities.Content> Families { get; set; }
        public List<Entities.Library> Libraries { get; set; }
        public List<Diff> DiffList { get; set; }
    }
}
