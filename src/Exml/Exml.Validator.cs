using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Schema;
using System.Collections.Generic;

using Exml.Logging;

namespace Exml
{

namespace Validator
{

public static class ExmlValidator
{
    public static List<ValidatorModel.ValidationIssue> Validate(string path)
    {
        using (var file = File.OpenRead(path))
        {
            return Validate(file);
        }
    }

    public static List<ValidatorModel.ValidationIssue> Validate(Stream stream)
    {
        // FIXME Maybe we should parametrize the settings
        var issues = new List<ValidatorModel.ValidationIssue>();
        var settings = new XmlReaderSettings();
        settings.ConformanceLevel = ConformanceLevel.Document;

        using (var reader = XmlReader.Create(stream, settings))
        {
            Stack<XmlModel.Widget> stack = new Stack<XmlModel.Widget>();
            XmlModel.Widget root = null;
            XmlModel.Widget current = null;

            try
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:

                            // FIXME: Guarantee there is no more than one root inside exml
                            if (reader.Name == "exml")
                            {
                                continue; // Skip the outer tag
                            }

                            var parent = current;

                            current = new XmlModel.Widget();

                            var constructingIssues = current.AddInfo(reader.Name, parent);
                            constructingIssues.ForEach(issue => issue.AddContext(reader as IXmlLineInfo));
                            issues.AddRange(constructingIssues);

                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    var attributeIssues = current.AddAttribute(reader.Name, reader.Value);
                                    attributeIssues.ForEach(issue => issue.AddContext(reader as IXmlLineInfo));
                                    issues.AddRange(attributeIssues);
                                }

                                reader.MoveToElement();
                            }

                            if (parent != null)
                            {
                                var parentIssues = parent.AddChild(current);
                                parentIssues.ForEach(issue => issue.AddContext(reader as IXmlLineInfo));
                                issues.AddRange(parentIssues);
                            }

                            if (root == null)
                            {
                                root = current;
                            }

                            if (!reader.IsEmptyElement)
                            {
                                stack.Push(current);
                            }
                            else
                            {
                                current = parent;
                                parent = current.Parent;
                            }

                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "exml")
                            {
                                continue; // Skip outer tag
                            }

                            current = stack.Pop();
                            parent = current.Parent;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (XmlException ex)
            {
                issues.Add(new ValidatorModel.ValidationIssue(
                            "Failed to read XML file.",
                            ex.Message,
                            ValidatorModel.ValidationIssueSeverity.CriticalError,
                            reader as IXmlLineInfo));
            }

            return issues;
        }
    }
}

}
}
