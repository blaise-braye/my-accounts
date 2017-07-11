using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Operations.Classification.GererMesComptes
{
    public class HtmlParser
    {
        public static Dictionary<string, string> ParseFieldsToDictionnary(string html, string formXpath)
        {
            HtmlNode.ElementsFlags.Remove("form");
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var formNode = document.DocumentNode.SelectSingleNode(formXpath);

            var fields = new Dictionary<string, string>();

            AddFieldsValues(formNode, fields);

            return fields;
        }

        private static void AddFieldsValues(HtmlNode node, Dictionary<string, string> fields)
        {
            var nodeName = node.Name.ToLower();

            switch (nodeName)
            {
                case "input":
                    AddInputValueToFields(node, fields);
                    break;
                case "select":
                    AddSelectValueToFields(node, fields);
                    break;
                case "textarea":
                    AddTextAreaValueToFields(node, fields);
                    break;
                default:
                    foreach (var childNode in node.ChildNodes)
                    {
                        AddFieldsValues(childNode, fields);
                    }

                    break;
            }
        }

        private static void AddInputValueToFields(HtmlNode node, Dictionary<string, string> fields)
        {
            var inputType = node.GetAttributeValue("type", string.Empty).ToLower();
            if (inputType.Equals("submit"))
            {
                return;
            }

            var fieldName = node.GetAttributeValue("name", string.Empty);
            var value = node.GetAttributeValue("value", string.Empty);
            fields.Add(fieldName, value);
        }

        private static void AddSelectValueToFields(HtmlNode node, Dictionary<string, string> fields)
        {
            var fieldName = node.GetAttributeValue("name", string.Empty);
            var valueNode =
                node.ChildNodes.FirstOrDefault(
                    n =>
                        n.Name.Equals("option", StringComparison.InvariantCultureIgnoreCase)
                        && !n.GetAttributeValue("selected", "notselected").Equals("notselected"));
            if (valueNode == null)
            {
                valueNode = node.ChildNodes.FirstOrDefault(n => n.Name.Equals("option", StringComparison.InvariantCultureIgnoreCase));
            }

            var value = valueNode?.GetAttributeValue("value", string.Empty) ?? string.Empty;
            fields.Add(fieldName, value);
        }

        private static void AddTextAreaValueToFields(HtmlNode node, Dictionary<string, string> fields)
        {
            var fieldName = node.GetAttributeValue("name", string.Empty);
            var value = node.InnerHtml;
            fields.Add(fieldName, value);
        }
    }
}