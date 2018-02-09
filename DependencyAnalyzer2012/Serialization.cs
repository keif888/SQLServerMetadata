//
// https://github.com/sabinio/SSISMSBuild/blob/master/SSIS2012Tasks/Serialization.cs
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.SqlServer.Dts.Runtime;

namespace Microsoft.Samples.DependencyAnalyzer
{
    public class Xml
    {
        public const string NS = "www.microsoft.com/SqlServer/SSIS";
        public const string Dts = "www.microsoft.com/SqlServer/Dts";
    }

    [XmlRootAttribute("Project", Namespace = Xml.NS)]
    public class ProjectManifest
    {
        [XmlAttribute(Namespace = Xml.NS, Form = XmlSchemaForm.Qualified)]
        public DTSProtectionLevel ProtectionLevel { get; set; }
        [XmlElement(Namespace = Xml.NS)]
        public Properties Properties { get; set; }
        public PackageManifest[] Packages { get; set; }
        public ConnectionManager[] ConnectionManagers { get; set; }
    }

    // [XmlType("Properties", Namespace = Xml.NS)]
    public class Properties : Dictionary<string, string>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool empty = reader.IsEmptyElement;
            reader.Read();
            if (empty)
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.LocalName == "Property")
                {
                    string name = reader.GetAttribute("Name", Xml.NS);
                    string value = reader.ReadString();

                    Add(name, value);

                    reader.ReadEndElement();
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [XmlType("Package", Namespace = Xml.NS)]
    public class PackageManifest
    {
        [XmlAttribute(Namespace = Xml.NS, Form = XmlSchemaForm.Qualified)]
        public string Name { get; set; }
        [XmlAttribute(Namespace = Xml.NS, Form = XmlSchemaForm.Qualified)]
        public bool EntryPoint { get; set; }
    }

    [XmlType("ConnectionManager", Namespace = Xml.NS)]
    public class ConnectionManager
    {
        [XmlAttribute(Namespace = Xml.NS, Form = XmlSchemaForm.Qualified)]
        public string Name { get; set; }
    }

    [XmlRoot("Parameters", Namespace = Xml.NS)]
    public class ProjectParameters : IXmlSerializable
    {
        public List<Parameter> Parameters { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public ProjectParameters()
        {
            Parameters = new List<Parameter>();
        }

        public void ReadXml(XmlReader reader)
        {
            bool empty = reader.IsEmptyElement;
            reader.Read();
            if (empty)
            {
                return;
            }

            var serializer = new XmlSerializer(typeof(Parameter));

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                string outerXml = reader.ReadOuterXml();

                var stringReader = new StringReader(outerXml);
                var parameter = (Parameter)serializer.Deserialize(stringReader);
                parameter.OuterXml = outerXml;

                Parameters.Add(parameter);
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    [XmlRoot("Parameter", Namespace = Xml.NS)]
    public class Parameter
    {
        [XmlAttribute(Namespace = Xml.NS, Form = XmlSchemaForm.Qualified)]
        public string Name { get; set; }

        [XmlElement(Namespace = Xml.NS)]
        public Properties Properties { get; set; }

        internal string OuterXml { get; set; }

        public XmlNode GetXml()
        {
            var document = new XmlDocument();
            document.LoadXml(OuterXml);

            return document.FirstChild;
        }
    }

    [XmlRoot("ConnectionManager", Namespace = Xml.Dts)]
    public class ProjectConnectionManager
    {
        [XmlAttribute(Namespace = Xml.Dts, Form = XmlSchemaForm.Qualified)]
        public string ObjectName { get; set; }

        [XmlAttribute(Namespace = Xml.Dts, Form = XmlSchemaForm.Qualified)]
        public string CreationName { get; set; }
    }
}