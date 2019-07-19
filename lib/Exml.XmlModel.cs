using System;
using System.Text;
using System.Collections.Generic;

using Exml.Logging;
using ApiModel = Exml.ApiModel;

namespace Exml
{

namespace XmlModel
{

// Object model for EXML interfaces
public class Widget
{

    private static ApiModel.API s_api;

    public string Name { get; set; }
    public Dictionary<string, string> Properties { get; set; }
    public Dictionary<string, string> EventHandlers { get; set; }
    public List<Widget> Children { get; set; }
    public Widget Parent { get; set; }

    private bool _is_container;
    private ApiModel.Class _class;

    public Widget()
    {
        Properties = new Dictionary<string, string>();
        EventHandlers = new Dictionary<string, string>();
        Children = new List<Widget>();
    }

    public List<ValidatorModel.ValidationIssue> AddInfo(string name, Widget parent)
    {
        // RULE -  is it a valid widget name?
        string internal_name = name;
        var issues = new List<ValidatorModel.ValidationIssue>();

        if (!internal_name.Contains("."))
        {
            internal_name = "Efl.Ui." + internal_name;
        }

        _class = s_api.Classes.Find(c => c.Name == internal_name);

        if (_class == null)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Unknown type {name}", "Type could not be found in the Efl.Ui namespace",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }

        // TODO: Is it a container?
        if (_class != null)
        {
            _is_container = _class.Interfaces.Find(i => i.Name == "Efl.IPack") != null;
        }

        Name = name;
        Parent = parent;

        return issues;
    }

    public List<ValidatorModel.ValidationIssue> AddAttribute(string name, string value)
    {
        // TODO: Rule: Does the property exist?
        // TODO: Rule: Is the property writable?
        // TODO: Rule: Is the value acceptable for the property?
        // TODO: Differentiate between events and properties
        return new List<ValidatorModel.ValidationIssue>();
    }

    public List<ValidatorModel.ValidationIssue> AddChild(Widget child)
    {
        // TODO: Can we add this child (Is this a container?)?
        var issues = new List<ValidatorModel.ValidationIssue>();
        if (!_is_container)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Type {Name} is not a container", "It can't have children",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }

        // We still add so we can track the invalid information further down
        Children.Add(child);
        return issues;
    }

    public override String ToString()
    {
        return ToString(0);
    }

    public string ToString(int indent)
    {
        var spaces = new String(' ', 4 * indent);
        var sb = new StringBuilder();

        sb.AppendLine(spaces + $"Widget: {Name}");

        foreach (var entry in Properties)
        {
            sb.AppendLine(spaces + $"    property: {entry.Key} => {entry.Value}");
        }

        if (Children.Count > 0)
        {
            foreach (var child in Children)
            {
                sb.AppendLine(spaces + child.ToString(indent + 1));
            }
        }

        return sb.ToString();
    }

    public static void SetApi(ApiModel.API api)
    {
        s_api = api;
    }
}

} // Model

} // Exml

