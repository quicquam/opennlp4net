using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace opennlp.tools.Tests.utils
{
    public static class Dumper
    {
        public static void Dump(object value, string name, TextWriter writer)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (writer == null)
                throw new ArgumentNullException("writer");
            var lookup = new Dictionary<object, int>();
            InternalDump(0, name, value, writer, lookup, true);
        }

        private static void InternalDump(int indentationLevel, string name, object value, TextWriter writer,
            Dictionary<object, int> lookup, bool recursiveDump)
        {
            var str1 = new string(' ', indentationLevel*3);
            if (value == null)
            {
                writer.WriteLine("{0}{1} = <null>", str1, name);
            }
            else
            {
                Type type = value.GetType();
                string str2 = string.Empty;
                string str3 = string.Empty;
                if (!type.IsValueType)
                {
                    int num1;
                    if (lookup.TryGetValue(value, out num1))
                    {
                        str2 = string.Format(CultureInfo.InvariantCulture, " (see #{0})", new object[1]
                        {
                            num1
                        });
                    }
                    else
                    {
                        int num2 = lookup.Count + 1;
                        lookup[value] = num2;
                        str3 = string.Format(CultureInfo.InvariantCulture, "#{0}: ", new object[1]
                        {
                            num2
                        });
                    }
                }
                bool flag = value is string;
                string fullName = value.GetType().FullName;
                string str4 = value.ToString();
                var exception = value as Exception;
                if (exception != null)
                    str4 = exception.GetType().Name + ": " + exception.Message;
                string str5;
                if (str4 == fullName)
                {
                    str5 = string.Empty;
                }
                else
                {
                    string str6 = str4.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");
                    if (flag)
                        str6 = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[1]
                        {
                            str6
                        });
                    str5 = " = " + str6;
                }
                writer.WriteLine("{0}{1}{2}{3} [{4}]{5}", (object) str1, (object) str3, (object) name, (object) str5,
                    (object) value.GetType(), (object) str2);
                if (str2.Length > 0 || flag || type.IsValueType && type.FullName == "System." + type.Name ||
                    !recursiveDump)
                    return;
                PropertyInfo[] propertyInfoArray =
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(property =>
                        {
                            if (property.GetIndexParameters().Length == 0)
                                return property.CanRead;
                            return false;
                        }).ToArray();
                FieldInfo[] fieldInfoArray =
                    type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToArray();
                if (propertyInfoArray.Length == 0 && fieldInfoArray.Length == 0)
                    return;
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}{{", new object[1]
                {
                    str1
                }));
                if (propertyInfoArray.Length > 0)
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}   properties {{", new object[1]
                    {
                        str1
                    }));
                    foreach (PropertyInfo propertyInfo in propertyInfoArray)
                    {
                        try
                        {
                            object obj = propertyInfo.GetValue(value, null);
                            InternalDump(indentationLevel + 2, propertyInfo.Name, obj, writer, lookup, true);
                        }
                        catch (TargetInvocationException ex)
                        {
                            InternalDump(indentationLevel + 2, propertyInfo.Name, ex, writer, lookup, false);
                        }
                    }
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}   }}", new object[1]
                    {
                        str1
                    }));
                }
                if (fieldInfoArray.Length > 0)
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}   fields {{", new object[1]
                    {
                        str1
                    }));
                    foreach (FieldInfo fieldInfo in fieldInfoArray)
                    {
                        try
                        {
                            object obj = fieldInfo.GetValue(value);
                            InternalDump(indentationLevel + 2, fieldInfo.Name, obj, writer, lookup, true);
                        }
                        catch (TargetInvocationException ex)
                        {
                            InternalDump(indentationLevel + 2, fieldInfo.Name, ex, writer, lookup, false);
                        }
                    }
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}   }}", new object[1]
                    {
                        str1
                    }));
                }
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}}}", new object[1]
                {
                    str1
                }));
            }
        }
    }
}