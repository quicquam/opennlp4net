using System;
using System.Collections;
using System.Collections.Generic;
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
using System.IO;
using j4n.Exceptions;
using opennlp.tools.namefind;

namespace opennlp.tools.cmdline
{


	using BioNLP2004NameSampleStreamFactory = opennlp.tools.formats.BioNLP2004NameSampleStreamFactory<NameSample>;
	using ChunkerSampleStreamFactory = opennlp.tools.formats.ChunkerSampleStreamFactory;
	using Conll02NameSampleStreamFactory = opennlp.tools.formats.Conll02NameSampleStreamFactory;
	using Conll03NameSampleStreamFactory = opennlp.tools.formats.Conll03NameSampleStreamFactory;
	using ConllXPOSSampleStreamFactory = opennlp.tools.formats.ConllXPOSSampleStreamFactory;
	using ConllXSentenceSampleStreamFactory = opennlp.tools.formats.ConllXSentenceSampleStreamFactory;
	using ConllXTokenSampleStreamFactory = opennlp.tools.formats.ConllXTokenSampleStreamFactory;
	using CorefSampleStreamFactory = opennlp.tools.formats.CorefSampleStreamFactory;
	using DocumentSampleStreamFactory = opennlp.tools.formats.DocumentSampleStreamFactory;
	using LeipzigDocumentSampleStreamFactory = opennlp.tools.formats.LeipzigDocumentSampleStreamFactory;
	using NameSampleDataStreamFactory = opennlp.tools.formats.NameSampleDataStreamFactory;
	using ParseSampleStreamFactory = opennlp.tools.formats.ParseSampleStreamFactory;
	using SentenceSampleStreamFactory = opennlp.tools.formats.SentenceSampleStreamFactory;
	using TokenSampleStreamFactory = opennlp.tools.formats.TokenSampleStreamFactory;
	using WordTagSampleStreamFactory = opennlp.tools.formats.WordTagSampleStreamFactory;
	using ADChunkSampleStreamFactory = opennlp.tools.formats.ad.ADChunkSampleStreamFactory;
	using ADNameSampleStreamFactory = opennlp.tools.formats.ad.ADNameSampleStreamFactory;
	using ADPOSSampleStreamFactory = opennlp.tools.formats.ad.ADPOSSampleStreamFactory;
	using ADSentenceSampleStreamFactory = opennlp.tools.formats.ad.ADSentenceSampleStreamFactory;
	using ADTokenSampleStreamFactory = opennlp.tools.formats.ad.ADTokenSampleStreamFactory;
	using NameToSentenceSampleStreamFactory = opennlp.tools.formats.convert.NameToSentenceSampleStreamFactory;
	using NameToTokenSampleStreamFactory = opennlp.tools.formats.convert.NameToTokenSampleStreamFactory;
	using POSToSentenceSampleStreamFactory = opennlp.tools.formats.convert.POSToSentenceSampleStreamFactory;
	using POSToTokenSampleStreamFactory = opennlp.tools.formats.convert.POSToTokenSampleStreamFactory;
	using ParseToPOSSampleStreamFactory = opennlp.tools.formats.convert.ParseToPOSSampleStreamFactory;
	using ParseToSentenceSampleStreamFactory = opennlp.tools.formats.convert.ParseToSentenceSampleStreamFactory;
	using ParseToTokenSampleStreamFactory = opennlp.tools.formats.convert.ParseToTokenSampleStreamFactory;
//	using ConstitParseSampleStreamFactory = opennlp.tools.formats.frenchtreebank.ConstitParseSampleStreamFactory;
	using Muc6FullParseCorefSampleStreamFactory = opennlp.tools.formats.muc.Muc6FullParseCorefSampleStreamFactory;
	using Muc6NameSampleStreamFactory = opennlp.tools.formats.muc.Muc6NameSampleStreamFactory;

	/// <summary>
	/// Registry for object stream factories.
	/// </summary>
	public sealed class StreamFactoryRegistry<T>
	{

        private static readonly IDictionary<Type, IDictionary<string, ObjectStreamFactory<T>>> registry = new Dictionary<Type, IDictionary<string, ObjectStreamFactory<T>>>();

	  static StreamFactoryRegistry()
	  {
		ChunkerSampleStreamFactory.registerFactory();
		DocumentSampleStreamFactory.registerFactory();
		NameSampleDataStreamFactory.registerFactory();
		ParseSampleStreamFactory.registerFactory();
		SentenceSampleStreamFactory.registerFactory();
		TokenSampleStreamFactory.registerFactory();
		WordTagSampleStreamFactory.registerFactory();
		CorefSampleStreamFactory.registerFactory();

		NameToSentenceSampleStreamFactory.registerFactory();
		NameToTokenSampleStreamFactory.registerFactory();

		POSToSentenceSampleStreamFactory.registerFactory();
		POSToTokenSampleStreamFactory.registerFactory();

		ParseToPOSSampleStreamFactory.registerFactory();
		ParseToSentenceSampleStreamFactory.registerFactory();
		ParseToTokenSampleStreamFactory.registerFactory();

		BioNLP2004NameSampleStreamFactory.registerFactory();
		Conll02NameSampleStreamFactory.registerFactory();
		Conll03NameSampleStreamFactory.registerFactory();
		ConllXPOSSampleStreamFactory.registerFactory();
		ConllXSentenceSampleStreamFactory.registerFactory();
		ConllXTokenSampleStreamFactory.registerFactory();
		LeipzigDocumentSampleStreamFactory.registerFactory();
		ADChunkSampleStreamFactory.registerFactory();
		ADNameSampleStreamFactory.registerFactory();
		ADSentenceSampleStreamFactory.registerFactory();
		ADPOSSampleStreamFactory.registerFactory();
		ADTokenSampleStreamFactory.registerFactory();

		Muc6NameSampleStreamFactory.registerFactory();
		Muc6FullParseCorefSampleStreamFactory.registerFactory();

        // ConstitParser uses saxlib, excluded for the moment MJJ 14/11/2014
		// ConstitParseSampleStreamFactory.registerFactory();
	  }

	  public const string DEFAULT_FORMAT = "opennlp";

	  private StreamFactoryRegistry()
	  {
		// not intended to be instantiated
	  }

	  /// <summary>
	  /// Registers <param>factory</param> which reads format named <param>formatName</param> and
	  /// instantiates streams producing objects of <param>sampleClass</param> class.
	  /// </summary>
	  /// <param name="sampleClass"> class of the objects, produced by the streams instantiated by the factory </param>
	  /// <param name="formatName">  name of the format </param>
	  /// <param name="factory">     instance of the factory </param>
	  /// <returns> true if the factory was successfully registered </returns>
	  public static bool registerFactory(Type sampleClass, string formatName, ObjectStreamFactory<T> factory)
	  {
		bool result;
		IDictionary<string, ObjectStreamFactory<T>> formats = registry[sampleClass];
		if (null == formats)
		{
		  formats = new Dictionary<string, ObjectStreamFactory<T>>();
		}
		if (!formats.ContainsKey(formatName))
		{
		  formats[formatName] = factory;
		  registry[sampleClass] = formats;
		  result = true;
		}
		else
		{
		  result = false;
		}
		return result;
	  }

	  /// <summary>
	  /// Unregisters a factory which reads format named <param>formatName</param> and
	  /// instantiates streams producing objects of <param>sampleClass</param> class.
	  /// </summary>
	  /// <param name="sampleClass"> class of the objects, produced by the streams instantiated by the factory </param>
	  /// <param name="formatName">  name of the format </param>
	  public static void unregisterFactory(Type sampleClass, string formatName)
	  {
          IDictionary<string, ObjectStreamFactory<T>> formats = registry[sampleClass];
		if (null != formats)
		{
		  if (formats.ContainsKey(formatName))
		  {
			formats.Remove(formatName);
		  }
		}
	  }

	  /// <summary>
	  /// Returns all factories which produce objects of <param>sampleClass</param> class.
	  /// </summary>
	  /// <param name="sampleClass"> class of the objects, produced by the streams instantiated by the factory </param>
	  /// <returns> formats mapped to factories </returns>

	  public static IDictionary<string, ObjectStreamFactory<T>> getFactories(Type sampleClass)
	  {
		return (IDictionary<string, ObjectStreamFactory<T>>)(object) registry[sampleClass];
	  }

	  /// <summary>
	  /// Returns a factory which reads format named <param>formatName</param> and
	  /// instantiates streams producing objects of <param>sampleClass</param> class.
	  /// </summary>
	  /// <param name="sampleClass"> class of the objects, produced by the streams instantiated by the factory </param>
	  /// <param name="formatName">  name of the format, if null, assumes OpenNLP format </param>
	  /// <returns> factory instance </returns>

      public static ObjectStreamFactory<T> getFactory(Type sampleClass, string formatName)
	  {
		if (null == formatName)
		{
		  formatName = DEFAULT_FORMAT;
		}

		ObjectStreamFactory<T> factory = registry.ContainsKey(sampleClass) ? registry[sampleClass][formatName] : null;

		if (factory != null)
		{
		  return factory;
		}
		else
		{
		  try
		  {
			Type factoryClazz = Type.GetType(formatName);

			// TODO: Need to check if it can produce the desired output
			// Otherwise there will be class cast exceptions later in the flow

			try
			{
			    if (factoryClazz != null) return (ObjectStreamFactory<T>) Activator.CreateInstance(factoryClazz);
			}
			catch (InstantiationException)
			{
				return null;
			}
			catch (IllegalAccessException)
			{
			  return null;
			}

		  }
		  catch (ClassNotFoundException)
		  {
			return null;
		  }
		}
	      return null;
	  }

	    public static void registerFactory(Type sampleClass, string formatName, ADSentenceSampleStreamFactory factory)
	    {
	        throw new NotImplementedException();
	    }
	}
}