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
        public int? PLCTagID { get; set; }
        [XmlElement("AssetName")]
        public string AssetName { get; set; }

        //Navigation Properties
        [XmlElement("Instance")]
        [ForeignKey("InstanceID")]
        public Instance Instance { get; set; }
        [XmlElement("Attribute")]
        [ForeignKey("AttributeID")]
        public Attribute Attribute { get; set; }
        [XmlElement("IOTag")]
        [ForeignKey("IOTagID")]
        public IOTag IOTag { get; set; }
        [XmlElement("PLCTag")]
        [ForeignKey("PLCTagID")]
        public PLCTag PLCTag { get; set; }


        public InstanceContent(string InstanceName, string AttributeName, string IOTagName, string PLCTagName) : this(InstanceName, AttributeName, IOTagName, PLCTagName,0,0,0)
        { }
        public InstanceContent(string _InstanceName, string _AttributeName, string _IOTagName, string _PLCTagName, int _Rack, int _Slot, int _Point)
        {
            Instance = new Instance(_InstanceName);
            Attribute = new Attribute(_AttributeName);
            IOTag = new IOTag(_IOTagName);
            PLCTag = new PLCTag(_PLCTagName){ Rack = _Rack, Slot = _Slot, Point = _Point};
        }
        public InstanceContent() { }
    }
}