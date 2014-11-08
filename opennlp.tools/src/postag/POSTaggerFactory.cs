using System;
using System.Collections.Generic;
using System.Linq;
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
using j4n.Interfaces;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.postag
{


	using AbstractModel = opennlp.model.AbstractModel;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using BaseToolFactory = opennlp.tools.util.BaseToolFactory;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using opennlp.tools.util;
	using ExtensionLoader = opennlp.tools.util.ext.ExtensionLoader<POSTaggerFactory>;
	using opennlp.tools.util.model;
	using UncloseableInputStream = opennlp.tools.util.model.UncloseableInputStream;

	/// <summary>
	/// The factory that provides POS Tagger default implementations and resources 
	/// </summary>
	public class POSTaggerFactory : BaseToolFactory
	{

	  private const string TAG_DICTIONARY_ENTRY_NAME = "tags.tagdict";
	  private const string NGRAM_DICTIONARY_ENTRY_NAME = "ngram.dictionary";

	  protected internal Dictionary ngramDictionary;
	  protected internal TagDictionary posDictionary;

	  /// <summary>
	  /// Creates a <seealso cref="POSTaggerFactory"/> that provides the default implementation
	  /// of the resources.
	  /// </summary>
	  public POSTaggerFactory()
	  {
	  }

	  /// <summary>
	  /// Creates a <seealso cref="POSTaggerFactory"/>. Use this constructor to
	  /// programmatically create a factory.
	  /// </summary>
	  /// <param name="ngramDictionary"> </param>
	  /// <param name="posDictionary"> </param>
	  public POSTaggerFactory(Dictionary ngramDictionary, TagDictionary posDictionary)
	  {
		this.init(ngramDictionary, posDictionary);
	  }

	  protected internal virtual void init(Dictionary ngramDictionary, TagDictionary posDictionary)
	  {
		this.ngramDictionary = ngramDictionary;
		this.posDictionary = posDictionary;
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("rawtypes") public java.util.Map<String, opennlp.tools.util.model.ArtifactSerializer> createArtifactSerializersMap()
	  public IDictionary<string, ArtifactSerializer<Object>> createArtifactSerializersMap()
	  {
		IDictionary<string, ArtifactSerializer<Object>> serializers = base.createArtifactSerializersMap();
		POSDictionarySerializer.register(serializers);
		// the ngram Dictionary uses a base serializer, we don't need to add it here.
		return serializers;
	  }

	  public override IDictionary<string, object> createArtifactMap()
	  {
		IDictionary<string, object> artifactMap = base.createArtifactMap();

		if (posDictionary != null)
		{
		  artifactMap[TAG_DICTIONARY_ENTRY_NAME] = posDictionary;
		}

		if (ngramDictionary != null)
		{
		  artifactMap[NGRAM_DICTIONARY_ENTRY_NAME] = ngramDictionary;
		}

		return artifactMap;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TagDictionary createTagDictionary(java.io.File dictionary) throws opennlp.tools.util.InvalidFormatException, java.io.FileNotFoundException, java.io.IOException
	  public virtual TagDictionary createTagDictionary(Jfile dictionary)
	  {
		return createTagDictionary(new FileInputStream(dictionary));
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TagDictionary createTagDictionary(java.io.InputStream in) throws opennlp.tools.util.InvalidFormatException, java.io.IOException
	  public virtual TagDictionary createTagDictionary(InputStream @in)
	  {
		return POSDictionary.create(@in);
	  }

	  public virtual TagDictionary TagDictionary
	  {
		  set
		  {
			if (artifactProvider != null)
			{
			  throw new IllegalStateException("Can not set tag dictionary while using artifact provider.");
			}
			this.posDictionary = value;
		  }
		  get
		  {
			if (this.posDictionary == null && artifactProvider != null)
			{
			  this.posDictionary = artifactProvider.getArtifact<TagDictionary>(TAG_DICTIONARY_ENTRY_NAME);
			}
			return this.posDictionary;
		  }
	  }


	  public virtual Dictionary Dictionary
	  {
		  get
		  {
			if (this.ngramDictionary == null && artifactProvider != null)
			{
			  this.ngramDictionary = artifactProvider.getArtifact<Dictionary>(NGRAM_DICTIONARY_ENTRY_NAME);
			}
			return this.ngramDictionary;
		  }
		  set
		  {
			if (artifactProvider != null)
			{
			  throw new IllegalStateException("Can not set ngram dictionary while using artifact provider.");
			}
			this.ngramDictionary = value;
		  }
	  }


	  public virtual POSContextGenerator POSContextGenerator
	  {
		  get
		  {
			return new DefaultPOSContextGenerator(0, Dictionary);
		  }
	  }

	  public virtual POSContextGenerator getPOSContextGenerator(int cacheSize)
	  {
		return new DefaultPOSContextGenerator(cacheSize, Dictionary);
	  }

	  public virtual SequenceValidator<string> SequenceValidator
	  {
		  get
		  {
			return new DefaultPOSSequenceValidator(TagDictionary);
		  }
	  }

	  internal class POSDictionarySerializer : ArtifactSerializer<POSDictionary>
	  {

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public POSDictionary create(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
		public virtual POSDictionary create(InputStream @in)
		{
		  return POSDictionary.create(new UncloseableInputStream(@in));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void serialize(POSDictionary artifact, java.io.OutputStream out) throws java.io.IOException
		public virtual void serialize(POSDictionary artifact, OutputStream @out)
		{
		  artifact.serialize(@out);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") static void register(java.util.Map<String, opennlp.tools.util.model.ArtifactSerializer> factories)
		internal static void register(IDictionary<string, ArtifactSerializer<Object>> factories)
		{
            throw new NotImplementedException();
            // was factories["tagdict"] = new POSDictionarySerializer();
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void validatePOSDictionary(POSDictionary posDict, opennlp.model.AbstractModel posModel) throws opennlp.tools.util.InvalidFormatException
	  protected internal virtual void validatePOSDictionary(POSDictionary posDict, AbstractModel posModel)
	  {
		HashSet<string> dictTags = new HashSet<string>();

		foreach (var tag in posDict.SelectMany(posDict.getTags))
		{
		    dictTags.Add(tag);
		}

		HashSet<string> modelTags = new HashSet<string>();

		for (int i = 0; i < posModel.NumOutcomes; i++)
		{
		  modelTags.Add(posModel.getOutcome(i));
		}

		if (!modelTags.containsAll(dictTags))
		{
		  StringBuilder unknownTag = new StringBuilder();
		  foreach (string d in dictTags)
		  {
			if (!modelTags.Contains(d))
			{
			  unknownTag.Append(d).Append(" ");
			}
		  }
		  throw new InvalidFormatException("Tag dictionary contains tags " + "which are unknown by the model! The unknown tags are: " + unknownTag.ToString());
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void validateArtifactMap() throws opennlp.tools.util.InvalidFormatException
	  public override void validateArtifactMap()
	  {

		// Ensure that the tag dictionary is compatible with the model

          object tagdictEntry = this.artifactProvider.getArtifact<POSDictionary>(TAG_DICTIONARY_ENTRY_NAME);

		if (tagdictEntry != null)
		{
		  if (tagdictEntry is POSDictionary)
		  {
			if (!this.artifactProvider.LoadedFromSerialized)
			{
			  AbstractModel posModel = this.artifactProvider.getArtifact<AbstractModel>(POSModel.POS_MODEL_ENTRY_NAME);
			  POSDictionary posDict = (POSDictionary) tagdictEntry;
			  validatePOSDictionary(posDict, posModel);
			}
		  }
		  else
		  {
			throw new InvalidFormatException("POSTag dictionary has wrong type!");
		  }
		}

		object ngramDictEntry = this.artifactProvider.getArtifact<Dictionary>(NGRAM_DICTIONARY_ENTRY_NAME);

		if (ngramDictEntry != null && !(ngramDictEntry is Dictionary))
		{
		  throw new InvalidFormatException("NGram dictionary has wrong type!");
		}

	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static POSTaggerFactory create(String subclassName, opennlp.tools.dictionary.Dictionary ngramDictionary, TagDictionary posDictionary) throws opennlp.tools.util.InvalidFormatException
	  public static POSTaggerFactory create(string subclassName, Dictionary ngramDictionary, TagDictionary posDictionary)
	  {
		if (subclassName == null)
		{
		  // will create the default factory
		  return new POSTaggerFactory(ngramDictionary, posDictionary);
		}
		try
		{
		  POSTaggerFactory theFactory = ExtensionLoader.instantiateExtension(subclassName);
		  theFactory.init(ngramDictionary, posDictionary);
		  return theFactory;
		}
		catch (Exception e)
		{
		  string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
		  Console.Error.WriteLine(msg);
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		  throw new InvalidFormatException(msg, e);
		}

	  }

	  public virtual TagDictionary createEmptyTagDictionary()
	  {
		this.posDictionary = new POSDictionary(true);
		return this.posDictionary;
	  }
	}

}