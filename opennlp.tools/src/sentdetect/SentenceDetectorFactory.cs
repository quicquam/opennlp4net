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

namespace opennlp.tools.sentdetect
{


	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using Factory = opennlp.tools.sentdetect.lang.Factory;
	using BaseToolFactory = opennlp.tools.util.BaseToolFactory;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using ExtensionLoader = opennlp.tools.util.ext.ExtensionLoader;

	/// <summary>
	/// The factory that provides SentenceDetecor default implementations and
	/// resources
	/// </summary>
	public class SentenceDetectorFactory : BaseToolFactory
	{

	  private string languageCode;
	  private char[] eosCharacters;
	  private Dictionary abbreviationDictionary;
	  private bool? useTokenEnd = null;

	  private const string ABBREVIATIONS_ENTRY_NAME = "abbreviations.dictionary";
	  private const string EOS_CHARACTERS_PROPERTY = "eosCharacters";
	  private const string TOKEN_END_PROPERTY = "useTokenEnd";

	  /// <summary>
	  /// Creates a <seealso cref="SentenceDetectorFactory"/> that provides the default
	  /// implementation of the resources.
	  /// </summary>
	  public SentenceDetectorFactory()
	  {
	  }

	  /// <summary>
	  /// Creates a <seealso cref="SentenceDetectorFactory"/>. Use this constructor to
	  /// programmatically create a factory.
	  /// </summary>
	  /// <param name="languageCode"> </param>
	  /// <param name="abbreviationDictionary"> </param>
	  /// <param name="eosCharacters"> </param>
	  public SentenceDetectorFactory(string languageCode, bool useTokenEnd, Dictionary abbreviationDictionary, char[] eosCharacters)
	  {
		this.init(languageCode, useTokenEnd, abbreviationDictionary, eosCharacters);
	  }

	  protected internal virtual void init(string languageCode, bool useTokenEnd, Dictionary abbreviationDictionary, char[] eosCharacters)
	  {
		this.languageCode = languageCode;
		this.useTokenEnd = useTokenEnd;
		this.eosCharacters = eosCharacters;
		this.abbreviationDictionary = abbreviationDictionary;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void validateArtifactMap() throws opennlp.tools.util.InvalidFormatException
	  public override void validateArtifactMap()
	  {

		if (this.artifactProvider.getManifestProperty(TOKEN_END_PROPERTY) == null)
		{
		  throw new InvalidFormatException(TOKEN_END_PROPERTY + " is a mandatory property!");
		}

		object abbreviationsEntry = this.artifactProvider.getArtifact(ABBREVIATIONS_ENTRY_NAME);

		if (abbreviationsEntry != null && !(abbreviationsEntry is Dictionary))
		{
		  throw new InvalidFormatException("Abbreviations dictionary '" + abbreviationsEntry + "' has wrong type, needs to be of type Dictionary!");
		}
	  }

	  public override IDictionary<string, object> createArtifactMap()
	  {
		IDictionary<string, object> artifactMap = base.createArtifactMap();

		// Abbreviations are optional
		if (abbreviationDictionary != null)
		{
		  artifactMap[ABBREVIATIONS_ENTRY_NAME] = abbreviationDictionary;
		}

		return artifactMap;
	  }

	  public override IDictionary<string, string> createManifestEntries()
	  {
		IDictionary<string, string> manifestEntries = base.createManifestEntries();

		manifestEntries[TOKEN_END_PROPERTY] = Convert.ToString(UseTokenEnd);

		// EOS characters are optional
		if (EOSCharacters != null)
		{
		  manifestEntries[EOS_CHARACTERS_PROPERTY] = eosCharArrayToString(EOSCharacters);
		}

		return manifestEntries;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static SentenceDetectorFactory create(String subclassName, String languageCode, boolean useTokenEnd, opennlp.tools.dictionary.Dictionary abbreviationDictionary, char[] eosCharacters) throws opennlp.tools.util.InvalidFormatException
	  public static SentenceDetectorFactory create(string subclassName, string languageCode, bool useTokenEnd, Dictionary abbreviationDictionary, char[] eosCharacters)
	  {
		if (subclassName == null)
		{
		  // will create the default factory
		  return new SentenceDetectorFactory(languageCode, useTokenEnd, abbreviationDictionary, eosCharacters);
		}
		try
		{
		  SentenceDetectorFactory theFactory = ExtensionLoader.instantiateExtension(typeof(SentenceDetectorFactory), subclassName);
		  theFactory.init(languageCode, useTokenEnd, abbreviationDictionary, eosCharacters);
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

	  public virtual char[] EOSCharacters
	  {
		  get
		  {
			if (this.eosCharacters == null)
			{
			  if (artifactProvider != null)
			  {
				string prop = this.artifactProvider.getManifestProperty(EOS_CHARACTERS_PROPERTY);
				if (prop != null)
				{
				  this.eosCharacters = eosStringToCharArray(prop);
				}
			  }
			  else
			  {
				// get from language dependent factory
				Factory f = new Factory();
				this.eosCharacters = f.getEOSCharacters(languageCode);
			  }
			}
			return this.eosCharacters;
		  }
	  }

	  public virtual bool UseTokenEnd
	  {
		  get
		  {
			if (this.useTokenEnd == null && artifactProvider != null)
			{
			  this.useTokenEnd = Convert.ToBoolean(artifactProvider.getManifestProperty(TOKEN_END_PROPERTY));
			}
			return this.useTokenEnd.Value;
		  }
	  }

	  public virtual Dictionary AbbreviationDictionary
	  {
		  get
		  {
			if (this.abbreviationDictionary == null && artifactProvider != null)
			{
			  this.abbreviationDictionary = artifactProvider.getArtifact(ABBREVIATIONS_ENTRY_NAME);
			}
			return this.abbreviationDictionary;
		  }
	  }

	  public virtual string LanguageCode
	  {
		  get
		  {
			if (this.languageCode == null && artifactProvider != null)
			{
			  this.languageCode = this.artifactProvider.Language;
			}
			return this.languageCode;
		  }
	  }

	  public virtual EndOfSentenceScanner EndOfSentenceScanner
	  {
		  get
		  {
			Factory f = new Factory();
			char[] eosChars = EOSCharacters;
			if (eosChars != null && eosChars.Length > 0)
			{
			  return f.createEndOfSentenceScanner(eosChars);
			}
			else
			{
			  return f.createEndOfSentenceScanner(this.languageCode);
			}
		  }
	  }

	  public virtual SDContextGenerator SDContextGenerator
	  {
		  get
		  {
			Factory f = new Factory();
			char[] eosChars = EOSCharacters;
			HashSet<string> abbs = null;
			Dictionary abbDict = AbbreviationDictionary;
			if (abbDict != null)
			{
			  abbs = abbDict.asStringSet();
			}
			else
			{
			  abbs = Collections.emptySet();
			}
			if (eosChars != null && eosChars.Length > 0)
			{
			  return f.createSentenceContextGenerator(abbs, eosChars);
			}
			else
			{
			  return f.createSentenceContextGenerator(this.languageCode, abbs);
			}
		  }
	  }

	  private string eosCharArrayToString(char[] eosCharacters)
	  {
		StringBuilder eosString = new StringBuilder();
		eosString.Append(eosCharacters);
		return eosString.ToString();
	  }

	  private char[] eosStringToCharArray(string eosCharacters)
	  {
		return eosCharacters.ToCharArray();
	  }
	}

}