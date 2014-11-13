using System;
using System.Collections.Generic;
using System.Text;
/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using j4n.Exceptions;

namespace opennlp.tools.cmdline
{


    /// <summary>
    /// Parser for command line arguments. The parser creates a dynamic proxy which
    /// can be access via a command line argument interface.
    /// 
    /// <para>
    /// 
    /// The command line argument proxy interface must follow these conventions:<br>
    /// - Methods do not define arguments<br>
    /// - Method names must start with get<br>
    /// - Allowed return types are Integer, Boolean, String, File and Charset.<br>
    /// </para>
    /// <para>
    /// <b>Note:</b> Do not use this class, internal use only!
    /// </para>
    /// </summary>
    public class ArgumentParser
    {

	  public class OptionalParameter : System.Attribute
	  {
		  private readonly ArgumentParser outerInstance;

		  public OptionalParameter(ArgumentParser outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		public const string DEFAULT_CHARSET = "DEFAULT_CHARSET";
		public string defaultValue = "";
	  }

	  public class ParameterDescription : System.Attribute
	  {
		public string valueName ;
		public string description = "";
	  }
        /*
              private interface ArgumentFactory
              {

                object parseArgument(Method method, string argName, string argValue);
              }

              public static class ArgumentFactory_Fields
              {
                public static readonly string INVALID_ARG = "Invalid argument: %s %s \n";
              }

              private static class IntegerArgumentFactory : ArgumentFactory
              {

                public object parseArgument(Method method, string argName, string argValue)
                {

                  object value;

                  try
                  {
                    value = Convert.ToInt32(argValue);
                  }
                  catch (NumberFormatException e)
                  {
                    throw new TerminateToolException(1, string.Format(INVALID_ARG, argName, argValue) + "Value must be an integer!", e);
                  }

                  return value;
                }
              }

              private static class BooleanArgumentFactory : ArgumentFactory
              {

                public object parseArgument(Method method, string argName, string argValue)
                {
                  return Convert.ToBoolean(argValue);
                }
              }

              private static class StringArgumentFactory : ArgumentFactory
              {

                public object parseArgument(Method method, string argName, string argValue)
                {
                  return argValue;
                }
              }

              private static class FileArgumentFactory : ArgumentFactory
              {

                public object parseArgument(Method method, string argName, string argValue)
                {
                  return new File(argValue);
                }
              }

              private static class CharsetArgumentFactory : ArgumentFactory
              {

                public object parseArgument(Method method, string argName, string charsetName)
                {

                  try
                  {
                    if (OptionalParameter.DEFAULT_CHARSET.Equals(charsetName))
                    {
                      return Charset.defaultCharset();
                    }
                    else if (Charset.isSupported(charsetName))
                    {
                      return Charset.forName(charsetName);
                    }
                    else
                    {
                      throw new TerminateToolException(1, string.format(INVALID_ARG, argName, charsetName) + "Encoding not supported on this platform.");
                    }
                  }
                  catch (IllegalCharsetNameException)
                  {
                    throw new TerminateToolException(1, string.format(INVALID_ARG, argName, charsetName) + "Illegal encoding name.");
                  }
                }
              }

              private static class ArgumentProxy : InvocationHandler
              {

                private readonly IDictionary<string, object> arguments;

                ArgumentProxy(IDictionary<string, object> arguments)
                {
                  this.arguments = arguments;
                }

                public object invoke(object proxy, Method method, Object[] args) throws Exception
                {

                  if (args != null)
                  {
                    throw new IllegalStateException();
                  }

                  return arguments.get(method.Name);
                }
              }

              private static final IDictionary<Type, ArgumentFactory> argumentFactories;

              static ArgumentParser()
              {
                IDictionary<Type, ArgumentFactory> factories = new Dictionary<Type, ArgumentParser.ArgumentFactory>();
                factories[typeof(int?)] = new IntegerArgumentFactory();
                factories[typeof(bool?)] = new BooleanArgumentFactory();
                factories[typeof(string)] = new StringArgumentFactory();
                factories[typeof(File)] = new FileArgumentFactory();
                factories[typeof(Charset)] = new CharsetArgumentFactory();

                argumentFactories = Collections.unmodifiableMap(factories);
              }

              private ArgumentParser()
              {
              }

              private static <T> void checkProxyInterfaces(Type... proxyInterfaces)
              {
                foreach (Type proxyInterface in proxyInterfaces)
                {
                  if (null != proxyInterface)
                  {
                    if (!proxyInterface.Interface)
                    {
                      throw new System.ArgumentException("proxy interface is not an interface!");
                    }

                    // all checks should also be performed for super interfaces

                    Method[] methods = proxyInterface.Methods;

                    if (methods.Length == 0)
                    {
                      throw new System.ArgumentException("proxy interface must at least declare one method!");
                    }

                    foreach (Method method in methods)
                    {

                      // check that method names start with get
                      if (!method.Name.StartsWith("get", StringComparison.Ordinal) && method.Name.length() > 3)
                      {
                        throw new System.ArgumentException(method.Name + " method name does not start with 'get'!");
                      }

                      // check that method has zero arguments
                      if (method.ParameterTypes.length != 0)
                      {
                        throw new System.ArgumentException(method.Name + " method must have zero parameters but has " + method.ParameterTypes.length + "!");
                      }

                      // check return types of interface
                      Type returnType = method.ReturnType;

                      IDictionary<Type, ArgumentFactory>.KeyCollection compatibleReturnTypes = argumentFactories.Keys;

                      if (!compatibleReturnTypes.contains(returnType))
                      {
                         throw new System.ArgumentException(method.Name + " method must have compatible return type! Got " + returnType + ", expected one of " + compatibleReturnTypes);
                      }
                    }
                  }
                }
              }

              private static string methodNameToParameter(string methodName)
              {
                // remove get from method name
                char[] parameterNameChars = methodName.ToCharArray();

                // name length is checked to be at least 4 prior
                parameterNameChars[3] = char.ToLower(parameterNameChars[3]);

                string parameterName = "-" + (new string(parameterNameChars)).Substring(3);

                return parameterName;
              }

              /// <summary>
              /// Creates a usage string which can be printed in case the user did specify the arguments
              /// incorrectly. Incorrectly is defined as <seealso cref="ArgumentParser#validateArguments(String[], Class)"/>
              /// returns false.
              /// </summary>
              /// <param name="argProxyInterface"> interface with parameter descriptions </param>
              /// <returns> the help message usage string </returns>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings({"unchecked"}) public static <T> String createUsage(Class argProxyInterface)
              public static <T> string createUsage(Type argProxyInterface)
              {
                return createUsage(new Type[]{argProxyInterface});
              }


              /// <summary>
              /// Creates a usage string which can be printed in case the user did specify the arguments
              /// incorrectly. Incorrectly is defined as {@link ArgumentParser#validateArguments(String[],
              /// Class[])}
              /// returns false.
              /// </summary>
              /// <param name="argProxyInterfaces"> interfaces with parameter descriptions </param>
              /// <returns> the help message usage string </returns>
              public static <T> string createUsage(Type... argProxyInterfaces)
              {
                checkProxyInterfaces(argProxyInterfaces);

                HashSet<string> duplicateFilter = new HashSet<string>();

                StringBuilder usage = new StringBuilder();
                StringBuilder details = new StringBuilder();
                foreach (Type argProxyInterface in argProxyInterfaces)
                {
                  if (null != argProxyInterface)
                  {
                    foreach (Method method in argProxyInterface.Methods)
                    {

                      ParameterDescription desc = method.getAnnotation(typeof(ParameterDescription));

                      OptionalParameter optional = method.getAnnotation(typeof(OptionalParameter));

                      if (desc != null)
                      {
                        string paramName = methodNameToParameter(method.Name);

                        if (duplicateFilter.Contains(paramName))
                        {
                          continue;
                        }
                        else
                        {
                          duplicateFilter.Add(paramName);
                        }

                        if (optional != null)
                        {
                          usage.Append('[');
                        }

                        usage.Append(paramName).Append(' ').Append(desc.valueName());
                        details.Append('\t').Append(paramName).Append(' ').Append(desc.valueName()).Append('\n');
                        if (desc.description() != null && desc.description().length() > 0)
                        {
                          details.Append("\t\t").Append(desc.description()).Append('\n');
                        }

                        if (optional != null)
                        {
                          usage.Append(']');
                        }

                        usage.Append(' ');
                      }
                    }
                  }
                }

                if (usage.Length > 0)
                {
                  usage.Length = usage.Length - 1;
                }

                if (details.Length > 0)
                {
                  details.Length = details.Length - 1;
                  usage.Append("\n\nArguments description:\n").Append(details.ToString());
                }

                return usage.ToString();
              }

              /// <summary>
              /// Tests if the argument are correct or incorrect. Incorrect means, that mandatory arguments are missing or
              /// there are unknown arguments. The argument value itself can also be incorrect, but this
              /// is checked by the <seealso cref="ArgumentParser#parse(String[], Class)"/> method and reported accordingly.
              /// </summary>
              /// <param name="args"> command line arguments </param>
              /// <param name="argProxyInterface"> interface with parameters description </param>
              /// <returns> true, if arguments are valid </returns>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings({"unchecked"}) public static <T> boolean validateArguments(String args[] , Class argProxyInterface)
              public static <T> bool validateArguments(string args[] , Type argProxyInterface)
              {
                return validateArguments(args, new Type[]{argProxyInterface});
              }

              /// <summary>
              /// Tests if the argument are correct or incorrect. Incorrect means, that mandatory arguments are missing or
              /// there are unknown arguments. The argument value itself can also be incorrect, but this
              /// is checked by the <seealso cref="ArgumentParser#parse(String[], Class)"/> method and reported accordingly.
              /// </summary>
              /// <param name="args"> command line arguments </param>
              /// <param name="argProxyInterfaces"> interfaces with parameters description </param>
              /// <returns> true, if arguments are valid </returns>
              public static <T> bool validateArguments(string args[] , Type... argProxyInterfaces)
              {
                return null == validateArgumentsLoudly(args, argProxyInterfaces);
              }

              /// <summary>
              /// Tests if the arguments are correct or incorrect.
              /// </summary>
              /// <param name="args"> command line arguments </param>
              /// <param name="argProxyInterface"> interface with parameters description </param>
              /// <returns> null, if arguments are valid or error message otherwise </returns>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings({"unchecked"}) public static <T> String validateArgumentsLoudly(String args[] , Class argProxyInterface)
              public static <T> string validateArgumentsLoudly(string args[] , Type argProxyInterface)
              {
                return validateArgumentsLoudly(args, new Type[]{argProxyInterface});
              }

              /// <summary>
              /// Tests if the arguments are correct or incorrect.
              /// </summary>
              /// <param name="args"> command line arguments </param>
              /// <param name="argProxyInterfaces"> interfaces with parameters description </param>
              /// <returns> null, if arguments are valid or error message otherwise </returns>
              public static <T> string validateArgumentsLoudly(string args[] , Type... argProxyInterfaces)
              {
                // number of parameters must be always be even
                if (args.length % 2 != 0)
                {
                  return "Number of parameters must be always be even";
                }

                int argumentCount = 0;
                IList<string> parameters = new List<string>(Arrays.asList(args));

                foreach (Type argProxyInterface in argProxyInterfaces)
                {
                  foreach (Method method in argProxyInterface.Methods)
                  {
                    string paramName = methodNameToParameter(method.Name);
                    int paramIndex = CmdLineUtil.getParameterIndex(paramName, args);
                    string valueString = CmdLineUtil.getParameter(paramName, args);
                    if (valueString == null)
                    {
                      OptionalParameter optionalParam = method.getAnnotation(typeof(OptionalParameter));

                      if (optionalParam == null)
                      {
                        if (-1 < paramIndex)
                        {
                          return "Missing mandatory parameter value: " + paramName;
                        }
                        else
                        {
                          return "Missing mandatory parameter: " + paramName;
                        }
                      }
                      else
                      {
                        parameters.Remove("-" + paramName);
                      }
                    }
                    else
                    {
                      parameters.Remove(paramName);
                      parameters.Remove(valueString);
                      argumentCount++;
                    }
                  }
                }

                if (args.length / 2 > argumentCount)
                {
                  return "Unrecognized parameters encountered: " + parameters.ToString();
                }

                return null;
              }

              /// <summary>
              /// Parses the passed arguments and creates an instance of the proxy interface.
              /// <para>
              /// In case an argument value cannot be parsed a <seealso cref="TerminateToolException"/> is
              /// thrown which contains an error message which explains the problems.
              /// 
              /// </para>
              /// </summary>
              /// <param name="args"> arguments </param>
              /// <param name="argProxyInterface"> interface with parameters description
              /// </param>
              /// <returns> parsed parameters
              /// </returns>
              /// <exception cref="TerminateToolException"> if an argument value cannot be parsed. </exception>
              /// <exception cref="IllegalArgumentException"> if validateArguments returns false, if the proxy interface is not compatible. </exception>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> T parse(String args[] , Class argProxyInterface)
              public static <T> T parse(string args[] , Type argProxyInterface)
              {

                checkProxyInterfaces(argProxyInterface);

                if (!validateArguments(args, argProxyInterface))
                {
                  throw new System.ArgumentException("Passed args must be valid!");
                }

                IDictionary<string, object> arguments = new Dictionary<string, object>();

                foreach (Method method in argProxyInterface.Methods)
                {

                  string parameterName = methodNameToParameter(method.Name);
                  string valueString = CmdLineUtil.getParameter(parameterName, args);

                  if (valueString == null)
                  {
                    OptionalParameter optionalParam = method.getAnnotation(typeof(OptionalParameter));

                    if (optionalParam.defaultValue().length() > 0)
                    {
                      valueString = optionalParam.defaultValue();
                    }
                    else
                    {
                      valueString = null;
                    }
                  }

                  Type returnType = method.ReturnType;

                  object value;

                  if (valueString != null)
                  {
                    ArgumentFactory factory = argumentFactories[returnType];

                    if (factory == null)
                    {
                      throw new IllegalStateException("factory for '" + returnType + "' must not be null");
                    }

                    value = factory.parseArgument(method, parameterName, valueString);
                  }
                  else
                  {
                    value = null;
                  }

                  arguments[method.Name] = value;
                }

                return (T) java.lang.reflect.Proxy.newProxyInstance(argProxyInterface.ClassLoader, new Type[]{argProxyInterface}, new ArgumentProxy(arguments));
              }

              /// <summary>
              /// Filters arguments leaving only those pertaining to argProxyInterface.
              /// </summary>
              /// <param name="args"> arguments </param>
              /// <param name="argProxyInterface"> interface with parameters description </param>
              /// @param <T> T </param>
              /// <returns> arguments pertaining to argProxyInterface </returns>
              public static <T> String[] filter(string args[], Type argProxyInterface)
              {
                List<string> parameters = new List<string>(args.length);

                foreach (Method method in argProxyInterface.Methods)
                {

                  string parameterName = methodNameToParameter(method.Name);
                  int idx = CmdLineUtil.getParameterIndex(parameterName, args);
                  if (-1 < idx)
                  {
                    parameters.Add(parameterName);
                    string valueString = CmdLineUtil.getParameter(parameterName, args);
                    if (null != valueString)
                    {
                      parameters.Add(valueString);
                    }
                  }
                }

                return parameters.ToArray();
              } */

        public static string[] filter(string[] args, Type p1)
        {
            throw new NotImplementedException();
        }
    }
}