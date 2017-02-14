using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace QuickSetup.Logic.Models
{
    [Serializable]
    public class AppsListModel
    {
        [XmlElement]
        public List<SingleSoftwareModel> AppsList { get; set; }
    }
}