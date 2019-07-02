using System;
using System.Reflection;
using System.Collections.Generic;

using ApiDump.Logging;

namespace ApiDump
{
namespace Model
{
    using ClassName = String;
    using EnumField = String;

    // Notes:
    // * Name always must be the fully qualified name of the entity.

    public enum ParameterDirection
    {
        In,
        Out,
        Inout,
        Unkown,
    }

    public class TypeRef
    {
        public string Name { get; private set; }

        public static TypeRef From(System.Type type)
        {
            var obj = new TypeRef();
            obj.Name = type.FullName;

            return obj;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Parameter
    {
        public string Name { get; private set; }
        public TypeRef Type { get; private set; }
        public ParameterDirection Direction { get; private set; }
    }

    public class Function
    {
        public string Name { get; private set; }
        public TypeRef ReturnType { get; private set; }
        public List<Parameter> Parameters { get; private set; }

        public static Function From(System.Type type)
        {
            Logger.Info($"Creating function from type: {type.FullName}");
            var obj = new Function();
            obj.Name = type.FullName;

            return obj;
        }
    }

    public class Enum
    {
        public string Name { get; private set; }
        public List<EnumField> Fields {get; private set; }

        public Enum()
        {
            Fields = new List<EnumField>();
        }

        public static Enum From(System.Type type)
        {
            Logger.Info($"Creating enum from type: {type.FullName}");
            var obj = new Enum();
            obj.Name = type.FullName;

            foreach(var field in type.GetFields())
            {
                // Avoid the `value__` field
                if (field.IsLiteral)
                {
                    obj.Fields.Add(field.Name);
                }
            }

            return obj;
        }
    }

    public class StructField
    {
        public string Name { get; private set; }
        public TypeRef Type { get; private set; }

        public static StructField From(FieldInfo field)
        {
            var ret = new StructField();
            ret.Name = field.Name;
            ret.Type = TypeRef.From(field.FieldType);

            return ret;
        }
    }

    public class Struct
    {
        public string Name { get; private set; }
        public List<StructField> Fields { get; private set; }

        public Struct()
        {
            Fields = new List<StructField>();
        }

        public static Struct From(System.Type type)
        {
            Logger.Info($"Creating struct from type: {type.FullName}");
            var obj = new Struct();
            obj.Name = type.FullName;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach(var field in fields)
            {
                obj.Fields.Add(StructField.From(field));
            }

            return obj;
        }
    }

    public class Class
    {
        public string Name { get; private set; }
        public List<Function> Constructors { get; private set; }
        public List<Function> Methods { get; private set; }
        public TypeRef Parent { get; private set; }
        public List<TypeRef> Interfaces { get; private set; }

        public bool IsInterface { get; private set; }
        public bool IsAbstract { get; private set; }

        public static Class From(System.Type type)
        {
            Logger.Info($"Creating class from type: {type.FullName}");
            var obj = new Class();
            obj.Name = type.FullName;

            return obj;
        }
    }
}
}