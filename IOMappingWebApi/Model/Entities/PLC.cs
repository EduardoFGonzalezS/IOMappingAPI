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
    public class PLC : ISimpleRecord
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [XmlElement("ID")]
        public int ID { get; set; }
        [XmlElement("Name")]
        public string Name { get; set; }

        //Navigation Properties
        [JsonIgnore]
        [XmlIgnore()]
        public ICollection<PLCTag> PLCTag { get; set; }
        [JsonIgnore]
        [XmlIgnore()]
        public ICollection<IOTag> IOTag { get; set; }

        public PLC(string _Name) { Name = _Name; }
        public PLC() { }
    }
}
