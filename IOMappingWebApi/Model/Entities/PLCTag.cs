using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using IOMappingWebApi.Model.Misc;
using System.Xml.Serialization;

namespace IOMappingWebApi.Model
{
    public class PLCTag : ISimpleRecord
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [XmlElement("ID")]
        public int ID { get; set; }
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Rack")]
        public int Rack { get; set; }
        [XmlElement("Slot")]
        public int Slot { get; set; }
        [XmlElement("Point")]
        public int Point { get; set; }

        //Navigation Property
        [JsonIgnore]
        [XmlIgnore()]
        public ICollection<InstanceContent> InstanceContent { get; set; }
    }
}