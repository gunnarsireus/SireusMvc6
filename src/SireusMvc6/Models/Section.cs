using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SireusMvc6.Models
{
    public class Section
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        //public string SectionType { get; set; }

        //public List<string> AllowedTypes { get; set; }
    }
}
