using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IOMappingWebApi.Model
{
    public class GalaxyObjects 
    {
        [XmlElement("List")]
        public List<InstanceContent> List { get; set; }

        public void Add(InstanceContent _InstanceContent)
        {
            List.Add(_InstanceContent);
        }
    }
}
