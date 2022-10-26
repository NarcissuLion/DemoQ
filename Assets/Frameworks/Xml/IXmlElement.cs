using System;
using System.Xml;

public interface IXmlElement
{
    void LoadData(XmlElement elem);
    void ExportData(XmlElement elem);
}
