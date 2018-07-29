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
        [XmlElement("PLCID")]
        public int PLCID { get; set; }

        //Navigation Property (So that it can be pointed from the InstanceContent Object)
        [JsonIgnore]
        [XmlIgnore()]
        public ICollection<InstanceContent> InstanceContent { get; set; }

        //Navigation Property (So that it can point to PLC Object)
        [XmlElement("PLC")]
        public PLC PLC { get; set; }

        public PLCTag(string _Name) { Name = _Name; }
        public PLCTag() { }
    }
}