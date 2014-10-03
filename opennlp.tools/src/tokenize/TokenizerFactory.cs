using System;
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
using j4n.Lang;
using opennlp.tools.util.ext;

namespace opennlp.tools.tokenize
{


	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using Factory = opennlp.tools.tokenize.lang.Factory;
	using BaseToolFactory = opennlp.tools.util.BaseToolFactory;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;

	/// <summary>
	/// The factory that provides <seealso cref="Tokenizer"/> default implementations and
	/// resources. Users can extend this class if their application requires
	/// overriding the <seealso cref="TokenContextGenerator"/>, <seealso cref="Dictionary"/> etc.
	/// </summary>
	public class TokenizerFactory : BaseToolFactory
	{

	  private string languageCode;
	  private Dictionary abbreviationDictionary;
	  private bool? useAlphaNumericOptimization = null;
	  private Pattern alphaNumericPattern;

	  private const string ABBREVIATIONS_ENTRY_NAME = "abbreviations.dictionary";
	  private const string USE_ALPHA_NUMERIC_OPTIMIZATION = "useAlphaNumericOptimization";
	  private const string ALPHA_NUMERIC_PATTERN = "alphaNumericPattern";

	  /// <summary>
	  /// Creates a <seealso cref="TokenizerFactory"/> that provides the default implementation
	  /// of the resources.
	  /// </summary>
	  public TokenizerFactory()
	  {
	  }

	  /// <summary>
	  /// Creates a <seealso cref="TokenizerFactory"/>. Use this constructor to
	  /// programmatically create a factory.
	  /// </summary>
	  /// <param name="languageCode">
	  ///          the language of the natural text </param>
	  /// <param name="abbreviationDictionary">
	  ///          an abbreviations dictionary </param>
	  /// <param name="useAlphaNumericOptimization">
	  ///          if true alpha numerics are skipped </param>
	  /// <param name="alphaNumericPattern">
	  ///          null or a custom alphanumeric pattern (default is:
	  ///          "^[A-Za-z0-9]+$", provided by <seealso cref="Factory#DEFAULT_ALPHANUMERIC"/> </param>
	  public TokenizerFactory(string languageCode, Dictionary abbreviationDictionary, bool useAlphaNumericOptimization, Pattern alphaNumericPattern)
	  {
		this.init(languageCode, abbreviationDictionary, useAlphaNumericOptimization, alphaNumericPattern);
	  }

	  protected internal virtual void init(string languageCode, Dictionary abbreviationDictionary, bool useAlphaNumericOptimization, Pattern alphaNumericPattern)
	  {
		this.languageCode = languageCode;
		this.useAlphaNumericOptimization = useAlphaNumericOptimization;
		this.alphaNumericPattern = alphaNumericPattern;
		this.abbreviationDictionary = abbreviationDictionary;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void validateArtifactMap() throws opennlp.tools.util.InvalidFormatException
	  public override void validateArtifactMap()
	  {

		if (this.artifactProvider.getManifestProperty(USE_ALPHA_NUMERIC_OPTIMIZATION) == null)
		{
		  throw new InvalidFormatException(USE_ALPHA_NUMERIC_OPTIMIZATION + " is a mandatory property!");
		}

		object abbreviationsEntry = this.artifactProvider.getArtifact<Tokenizer>(ABBREVIATIONS_ENTRY_NAME);

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

		manifestEntries[USE_ALPHA_NUMERIC_OPTIMIZATION] = Convert.ToString(UseAlphaNumericOptmization);

		// alphanumeric pattern is optional
		if (AlphaNumericPattern != null)
		{
		  manifestEntries[ALPHA_NUMERIC_PATTERN] = AlphaNumericPattern.pattern();
		}

		return manifestEntries;
	  }

	  /// <summary>
	  /// Factory method the framework uses create a new <seealso cref="TokenizerFactory"/>.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static TokenizerFactory create(String subclassName, String languageCode, opennlp.tools.dictionary.Dictionary abbreviationDictionary, boolean useAlphaNumericOptimization, java.util.regex.Pattern alphaNumericPattern) throws opennlp.tools.util.InvalidFormatException
	  public static TokenizerFactory create(string subclassName, string languageCode, Dictionary abbreviationDictionary, bool useAlphaNumericOptimization, Pattern alphaNumericPattern)
	  {
		if (subclassName == null)
		{
		  // will create the default factory
		  return new TokenizerFactory(languageCode, abbreviationDictionary, useAlphaNumericOptimization, alphaNumericPattern);
		}
		try
		{
          TokenizerFactory theFactory = ExtensionLoader<TokenizerFactory>.instantiateExtension(subclassName);
		  theFactory.init(languageCode, abbreviationDictionary, useAlphaNumericOptimization, alphaNumericPattern);
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

	  /// <summary>
	  /// Gets the alpha numeric pattern.
	  /// </summary>
	  /// <returns> the user specified alpha numeric pattern or a default. </returns>
	  public virtual Pattern AlphaNumericPattern
	  {
		  get
		  {
			if (this.alphaNumericPattern == null)
			{
			  if (artifactProvider != null)
			  {
				string prop = this.artifactProvider.getManifestProperty(ALPHA_NUMERIC_PATTERN);
				if (prop != null)
				{
				  this.alphaNumericPattern = Pattern.compile(prop);
				}
			  }
			  // could not load from manifest, will get from language dependent factory
			  if (this.alphaNumericPattern == null)
			  {
				Factory f = new Factory();
				this.alphaNumericPattern = f.getAlphanumeric(languageCode);
			  }
			}
			return this.alphaNumericPattern;
		  }
	  }

	  /// <summary>
	  /// Gets whether to use alphanumeric optimization.
	  /// </summary>
	  public virtual bool UseAlphaNumericOptmization
	  {
		  get
		  {
			if (this.useAlphaNumericOptimization == null && artifactProvider != null)
			{
			  this.useAlphaNumericOptimization = Convert.ToBoolean(artifactProvider.getManifestProperty(USE_ALPHA_NUMERIC_OPTIMIZATION));
			}
			return this.useAlphaNumericOptimization.Value;
		  }
	  }

	  /// <summary>
	  /// Gets the abbreviation dictionary
	  /// </summary>
	  /// <returns> null or the abbreviation dictionary </returns>
	  public virtual Dictionary AbbreviationDictionary
	  {
		  get
		  {
			if (this.abbreviationDictionary == null && artifactProvider != null)
			{
			  this.abbreviationDictionary = artifactProvider.getArtifact<Dictionary>(ABBREVIATIONS_ENTRY_NAME);
			}
			return this.abbreviationDictionary;
		  }
	  }

	  /// <summary>
	  /// Gets the language code
	  /// </summary>
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

	  /// <summary>
	  /// Gets the context generator
	  /// </summary>
	  public virtual TokenContextGenerator ContextGenerator
	  {
		  get
		  {
			Factory f = new Factory();
			HashSet<string> abbs = null;
			Dictionary abbDict = AbbreviationDictionary;
			if (abbDict != null)
			{
			  abbs = abbDict.asStringSet();
			}
			else
			{
			  abbs = new HashSet<string>();
			}
			return f.createTokenContextGenerator(LanguageCode, abbs);
		  }
	  }
	}

}