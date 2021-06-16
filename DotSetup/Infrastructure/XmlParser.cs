// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DotSetup.Infrastructure
{
    public static class XmlParser
    {
        public const string XPATH_REF_NAME = "Xpath-ref";
        public const string XPATH_ATTR_DEFAULT = "default";
        public const string KNOWN_PATH_NAME = "Known-path";
        public const string PROCESSOR_NAME = "Processor";
        public const string PROCESSOR_ATTR_ACTION = "action";
        public const string PROCESSOR_ATTR_VALUE = "value";
        public const int MAX_RECURSIVE_LEVEL = 10;

        public static string GetStringValue(XmlNode xmlNode)
        {
            string ret = string.Empty;
            try
            {
                if (xmlNode != null && xmlNode.HasChildNodes)
                {
                    ret = GetRecursiveStringValue(xmlNode.ChildNodes);
                }
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"Cannot retrieve the value of {xmlNode.LocalName}, \nerror: {e}, \nnode value: {xmlNode.InnerXml}");
#endif
            }


#if DEBUG
            Logger.GetLogger().Info((xmlNode == null ? "Unnamed" : xmlNode.Name) + "=" + ret, Logger.Level.HIGH_DEBUG_LEVEL);
#endif
            return ret;
        }

        public static string GetStringValue(XmlNode xmlNode, string key)
        {
            string ret = "";
            if (xmlNode != null)
            {
                if (xmlNode[key] != null)
                    ret = GetStringValue(xmlNode[key]);
                else if (string.IsNullOrEmpty(key))
                    ret = GetStringValue(xmlNode);
            }
            return ret;
        }

        public static List<string> GetStringsValues(XmlNodeList xmlNodeList)
        {
            List<string> stringsValues = new List<string>();

            foreach (XmlNode xmlNodechild in xmlNodeList)
            {
                stringsValues.Add(GetStringValue(xmlNodechild));
            }
            return stringsValues;
        }

        public static string GetStringAttribute(XmlNode xmlNode, string attribute)
        {
            string ret = "";
            if (xmlNode != null && xmlNode.Attributes != null && xmlNode.Attributes[attribute] != null)
            {
                ret = xmlNode.Attributes[attribute].Value;
            }
#if DEBUG
            Logger.GetLogger().Info(xmlNode.Name + "[" + attribute + "]=" + ret, Logger.Level.HIGH_DEBUG_LEVEL);
#endif
            return ret;
        }

        public static string GetStringAttribute(XmlNode xmlNode, string key, string attribute)
        {
            string ret = "";
            if (xmlNode != null && xmlNode[key] != null && xmlNode[key].Attributes != null && xmlNode[key].Attributes[attribute] != null)
            {
                ret = xmlNode[key].Attributes[attribute].Value;
            }
#if DEBUG
            Logger.GetLogger().Info(xmlNode.Name + "[" + attribute + "]=" + ret, Logger.Level.HIGH_DEBUG_LEVEL);
#endif
            return ret;
        }

        public static Dictionary<string, string> AddAllAttributes(Dictionary<string, string> dict, XmlNode xmlNode)
        {
            if (dict != null && xmlNode != null && xmlNode.Attributes != null)
                for (int i = 0; i < xmlNode.Attributes.Count; i++)
                    if (dict.ContainsKey(xmlNode.Attributes[i].Name))
                        dict[xmlNode.Attributes[i].Name] = xmlNode.Attributes[i].Value;
                    else
                        dict.Add(xmlNode.Attributes[i].Name, xmlNode.Attributes[i].Value);
            return dict;
        }

        public static int GetIntValue(XmlNode xmlNode, string key = "")
        {
            string strRes = GetStringValue(xmlNode, key);
            return string.IsNullOrEmpty(strRes) ? 0 : int.Parse(strRes);
        }

        public static int GetIntAttribute(XmlNode xmlNode, string key, string attribute)
        {
            string strRes = GetStringAttribute(xmlNode, key, attribute);
            return string.IsNullOrEmpty(strRes) ? 0 : int.Parse(strRes);
        }

        public static Color GetColorValue(XmlNode xmlNode, string key = "") => ColorTranslator.FromHtml(GetStringValue(xmlNode, key));

        public static bool GetBoolValue(XmlNode xmlNode, string key = "", bool ifEmpty = false)
        {
            string strRes = GetStringValue(xmlNode, key);
            if (!bool.TryParse(strRes, out bool result))
                result = ifEmpty;
            return result;
        }

        public static bool GetBoolAttribute(XmlNode xmlNode, string attribute, bool ifEmpty = false)
        {
            string strRes = GetStringAttribute(xmlNode, attribute);
            if (!bool.TryParse(strRes, out bool result))
                result = ifEmpty;
            return result;
        }

        public static Func<string, string> OnXpathProcessing = null;

        public static string GetRecursiveStringValue(XmlNodeList xmlChildNodes, int recursiveLevel = 0)
        {
            string res = string.Empty;

            foreach (XmlNode xmlNode in xmlChildNodes)
            {
                if (xmlNode == null)
                    continue;

                if (xmlNode.Name == XPATH_REF_NAME && xmlNode.HasChildNodes)
                {
                    if (recursiveLevel >= MAX_RECURSIVE_LEVEL)
                    {
#if DEBUG
                        Logger.GetLogger().Error($"Recursion deeper than {MAX_RECURSIVE_LEVEL} detected in path {xmlNode.Name}");
#endif
                        res = "...";
                    }
                    else
                    {
                        string XPath = GetRecursiveStringValue(xmlNode.ChildNodes, recursiveLevel + 1).Trim();
                        if (XPath == "")
                            continue;

                        string parsedXpath = OnXpathProcessing?.Invoke(XPath);

                        if (!string.IsNullOrEmpty(parsedXpath))
                        {
                            res += parsedXpath;
                        } 
						else
                        {
                            XmlNode newPathNode = xmlNode.SelectSingleNode(XPath);
                            if (newPathNode != null && newPathNode.HasChildNodes)
                                res += GetRecursiveStringValue(newPathNode.ChildNodes, recursiveLevel + 1);
                            else if (xmlNode.Attributes != null && xmlNode.Attributes[XPATH_ATTR_DEFAULT] != null)
                                res += xmlNode.Attributes[XPATH_ATTR_DEFAULT].Value;
                        }                       
                    }
                }
                else if (xmlNode.Name == KNOWN_PATH_NAME && xmlNode.HasChildNodes)
                {
                    res += KnownFolders.GetKnownPath(GetRecursiveStringValue(xmlNode.ChildNodes, recursiveLevel + 1));
                }
                else if (xmlNode.Name == PROCESSOR_NAME && xmlNode.HasChildNodes && xmlNode.Attributes[PROCESSOR_ATTR_ACTION] != null)
                {
                    string value = xmlNode.Attributes[PROCESSOR_ATTR_VALUE] != null ? xmlNode.Attributes[PROCESSOR_ATTR_VALUE].Value : string.Empty;
                    res += XmlProcessor.Process(xmlNode.Attributes[PROCESSOR_ATTR_ACTION].Value, GetRecursiveStringValue(xmlNode.ChildNodes, recursiveLevel + 1).Trim(), value);
                }
                else
                {
                    res += xmlNode.InnerText;
                }
            };

            return res;
        }

        public static Dictionary<string, string> GetXpathRefAttributes(XmlNode xmlNodeParent)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            AddAllAttributes(dict, xmlNodeParent);

            if (xmlNodeParent != null && xmlNodeParent.HasChildNodes)
            {
                foreach (XmlNode xmlNode in xmlNodeParent.ChildNodes)
                {
                    if (xmlNode != null && xmlNode.Name == XPATH_REF_NAME)
                    {
                        string XPath = string.Empty; ;
                        try
                        {
                            XPath = GetStringValue(xmlNode);
                            if (string.IsNullOrEmpty(XPath))
                                continue;
                            AddAllAttributes(dict, xmlNode.SelectSingleNode(XPath));
                        }
#if DEBUG
                        catch (Exception e)
#else
                        catch (Exception)
#endif
                        {
#if DEBUG
                            Logger.GetLogger().Error($"Cannot resolving X-Path: {XPath}, error: {e}");
#endif
                        }
                    }
                }
            }

            return dict;
        }

        public static Dictionary<string, string> GetChildNodesValues(XmlNode node)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();

            if (node == null)
                return res;

            if (node.ChildNodes.Count == 1)
            {
                string nodeName = string.IsNullOrWhiteSpace(node.ChildNodes[0].Name) ? "_" : node.ChildNodes[0].Name;
                string nodeValue = GetStringValue(node.ChildNodes[0]);
                if (string.IsNullOrWhiteSpace(nodeValue))
                    nodeValue = GetStringValue(node);
                res.Add(nodeName, nodeValue);
            }
            else
            {
                foreach (XmlNode childNode in node.ChildNodes)
                    res.Add(childNode.Name, GetStringValue(childNode));
            }

            return res;
        }


        public static void SetNode(XmlDocument doc, XmlNode newNode) => SetNode(doc, doc.DocumentElement, newNode);

        public static void SetNode(XmlDocument doc, XmlNode parent, XmlNode newNode, bool append = true)
        {
            bool hasMatch = false;
            foreach (XmlNode child in parent.ChildNodes)
            {
                if (FindXPath(child) == "/Main" + FindXPath(newNode))
                {
                    hasMatch = true;
                    MergeNodes(doc, child, newNode, append);
                }
            }
            if (!hasMatch)
            {
                if (append)
                    parent.AppendChild(doc.ImportNode(newNode, true));
                else
                    parent.PrependChild(doc.ImportNode(newNode, true));
            }
        }

        private static void MergeNodes(XmlDocument doc, XmlNode parent, XmlNode newNode, bool append = true)
        {

            foreach (XmlNode newNodeChild in newNode.ChildNodes)
            {
                bool hasMatch = false;
                foreach (XmlNode child in parent.ChildNodes)
                {
                    if (FindXPath(child) == "/Main" + FindXPath(newNodeChild) && newNodeChild.NodeType != XmlNodeType.Comment)
                    {
                        hasMatch = true;
                        parent.ReplaceChild(doc.ImportNode(newNodeChild, true), child);
                    }
                }
                if (!hasMatch)
                {
                    if (append)
                        parent.AppendChild(doc.ImportNode(newNodeChild, true));
                    else
                        parent.PrependChild(doc.ImportNode(newNodeChild, true));
                }
            }

        }

        internal static string FindXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        builder.Insert(0, "/" + node.Name);
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Comment:
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString();
                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        internal static int FindElementIndex(XmlElement element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlElement && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

        public static XmlNode MakeXPath(XmlDocument doc, string xpath)
        {
            XmlNode res = null;
            if (doc != null && doc.DocumentElement.HasChildNodes)
                res = MakeXPath(doc, doc.DocumentElement, xpath);
            return res;
        }

        public static XmlNode MakeXPath(XmlDocument doc, XmlNode parent, string xpath)
        {
            // grab the next node name in the xpath; or return parent if empty
            string[] partsOfXPath = xpath.Trim('/').Split('/');
            string nextNodeInXPath = partsOfXPath.First();
            if (string.IsNullOrEmpty(nextNodeInXPath))
                return parent;

            // get or create the node from the name
            XmlNode node = parent.SelectSingleNode(nextNodeInXPath);
            if (node == null)
                node = parent.AppendChild(doc.CreateElement(nextNodeInXPath));

            // rejoin the remainder of the array as an xpath expression and recurse
            string rest = string.Join("/", partsOfXPath.Skip(1).ToArray());
            return MakeXPath(doc, node, rest);
        }

        public static void SetStringValue(XmlDocument xmlDoc, string xpath, string newValue)
        {
            XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
            SetStringValue(xmlDoc, xmlNode, (xmlNode == null) ? xpath : string.Empty, newValue);
        }

        public static void SetStringValue(XmlDocument xmlDoc, XmlNode parent, string xpath, string newValue)
        {
            if (parent == null)
            {
                parent = MakeXPath(xmlDoc, xpath);
            }
            else
            {
                parent = MakeXPath(xmlDoc, parent, xpath);
            }

            if (parent != null)
                parent.InnerText = newValue;
        }

        public static XElement ToXElement(this XmlNode node)
        {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        public static XmlNode ToXmlNode(this XElement element)
        {
            using XmlReader xmlReader = element.CreateReader();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlReader);
            return xmlDoc;
        }
    }
}
