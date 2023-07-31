using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
//using static UnityEditor.Progress;

public class SignificadoCompleto
{
    public string word;
    public string xml;

    public XmlDocument GetXML()
    {
        xml = xml.Replace("\n", "");
        XmlDocument xmltest = new XmlDocument();
        xmltest.LoadXml(xml);

        return xmltest;
    }

    public string GetSignificado()
    {
        string retorno = "";
        XmlDocument xml = GetXML();

        XmlNode node = xml.SelectSingleNode("/entry/sense/def");
        retorno = node.InnerText;

        return retorno;
    }
}
