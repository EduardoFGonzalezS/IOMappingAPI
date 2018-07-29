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
    public class Instance : ISimpleRecord
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [XmlElement("ID")]
        public int ID { get; set; }
        [XmlElement("Name")]
        public string Name { get; set; }

        //Navigation Property
        [JsonIgnore]
        [XmlIgnore()]
        public ICollection<InstanceContent> InstanceContent { get; set; }

        public Instance(string _Name) { Name = _Name; }
        public Instance() { }
    }
}