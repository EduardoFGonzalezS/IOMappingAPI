using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace IOMappingWebApi.Model
{
    public class InstanceContent
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [XmlIgnore()]
        public int InstanceContentID { get; set; }
        [XmlIgnore()]
        public int InstanceID { get; set; }
        [XmlIgnore()]
        public int AttributeID { get; set; }
        [XmlIgnore()]
        public int IOTagID { get; set; }
        [XmlIgnore()]
        public int PLCTagID { get; set; }

        //Navigation Properties
        [XmlElement("Instance")]
        public Instance Instance { get; set; }
        [XmlElement("Attribute")]
        public Attribute Attribute { get; set; }
        [XmlElement("IOTag")]
        public IOTag IOTag { get; set; }
        [XmlElement("PLCTag")]
        public PLCTag PLCTag { get; set; }

    }
}