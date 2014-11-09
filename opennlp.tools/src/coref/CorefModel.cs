using System;

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


namespace opennlp.tools.coref
{


	using BinaryGISModelReader = opennlp.maxent.io.BinaryGISModelReader;
	using AbstractModel = opennlp.model.AbstractModel;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using StringList = opennlp.tools.util.StringList;
	using BaseModel = opennlp.tools.util.model.BaseModel;

	public class CorefModel : BaseModel
	{

	  private const string COMPONENT_NAME = "Coref";

	  private const string MALE_NAMES_DICTIONARY_ENTRY_NAME = "maleNames.dictionary";

	  private const string FEMALE_NAMES_DICTIONARY_ENTRY_NAME = "femaleNames.dictionary";

	  private const string NUMBER_MODEL_ENTRY_NAME = "number.model";

	//  private Map<String, Set<String>> acronyms;

	  private const string COMMON_NOUN_RESOLVER_MODEL_ENTRY_NAME = "commonNounResolver.model";

	  private const string DEFINITE_NOUN_RESOLVER_MODEL_ENTRY_NAME = "definiteNounResolver.model";

	  private const string SPEECH_PRONOUN_RESOLVER_MODEL_ENTRY_NAME = "speechPronounResolver.model";

	  // TODO: Add IModel

	  private const string PLURAL_NOUN_RESOLVER_MODEL_ENTRY_NAME = "pluralNounResolver.model";

	  private const string SINGULAR_PRONOUN_RESOLVER_MODEL_ENTRY_NAME = "singularPronounResolver.model";

	  private const string PROPER_NOUN_RESOLVER_MODEL_ENTRY_NAME = "properNounResolver.model";

	  private const string SIM_MODEL_ENTRY_NAME = "sim.model";

	  private const string PLURAL_PRONOUN_RESOLVER_MODEL_ENTRY_NAME = "pluralPronounResolver.model";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CorefModel(String languageCode, String project) throws java.io.IOException
	  public CorefModel(string languageCode, string project) : base(COMPONENT_NAME, languageCode, null)
	  {

		artifactMap[MALE_NAMES_DICTIONARY_ENTRY_NAME] = readNames(project + File.separator + "gen.mas");

		artifactMap[FEMALE_NAMES_DICTIONARY_ENTRY_NAME] = readNames(project + File.separator + "gen.fem");

		// TODO: Create acronyms

		artifactMap[NUMBER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "num.bin.gz");

		artifactMap[COMMON_NOUN_RESOLVER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "cmodel.bin.gz");

		artifactMap[DEFINITE_NOUN_RESOLVER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "defmodel.bin.gz");


		artifactMap[SPEECH_PRONOUN_RESOLVER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "fmodel.bin.gz");

		// TODO: IModel

		artifactMap[PLURAL_NOUN_RESOLVER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "plmodel.bin.gz");

		artifactMap[SINGULAR_PRONOUN_RESOLVER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "pmodel.bin.gz");

		artifactMap[PROPER_NOUN_RESOLVER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "pnmodel.bin.gz");

		artifactMap[SIM_MODEL_ENTRY_NAME] = createModel(project + File.separator + "sim.bin.gz");

		artifactMap[PLURAL_PRONOUN_RESOLVER_MODEL_ENTRY_NAME] = createModel(project + File.separator + "tmodel.bin.gz");

		checkArtifactMap();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private opennlp.model.AbstractModel createModel(String fileName) throws java.io.IOException
	  private AbstractModel createModel(string fileName)
	  {
		return (new BinaryGISModelReader(new DataInputStream(new GZIPInputStream(new FileInputStream(fileName))))).Model;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static opennlp.tools.dictionary.Dictionary readNames(String nameFile) throws java.io.IOException
	  private static Dictionary readNames(string nameFile)
	  {
		Dictionary names = new Dictionary();

		BufferedReader nameReader = new BufferedReader(new FileReader(nameFile));
		for (string line = nameReader.readLine(); line != null; line = nameReader.readLine())
		{
		  names.put(new StringList(line));
		}

		return names;
	  }

	  public virtual Dictionary MaleNames
	  {
		  get
		  {
			return (Dictionary) artifactMap[MALE_NAMES_DICTIONARY_ENTRY_NAME];
		  }
	  }

	  public virtual Dictionary FemaleNames
	  {
		  get
		  {
			return (Dictionary) artifactMap[FEMALE_NAMES_DICTIONARY_ENTRY_NAME];
		  }
	  }

	  public virtual AbstractModel NumberModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[NUMBER_MODEL_ENTRY_NAME];
		  }
	  }

	//  public AcronymDictionary getAcronyms() {
	//    return null;
	//  }

	  public virtual AbstractModel CommonNounResolverModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[COMMON_NOUN_RESOLVER_MODEL_ENTRY_NAME];
		  }
	  }

	  public virtual AbstractModel DefiniteNounResolverModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[DEFINITE_NOUN_RESOLVER_MODEL_ENTRY_NAME];
		  }
	  }

	  public virtual AbstractModel SpeechPronounResolverModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[SPEECH_PRONOUN_RESOLVER_MODEL_ENTRY_NAME];
		  }
	  }

	  // TODO: Where is this model used ?
	//  public AbstractModel getIModel() {
	//    return null;
	//  }

	  public virtual AbstractModel PluralNounResolverModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[PLURAL_NOUN_RESOLVER_MODEL_ENTRY_NAME];
		  }
	  }

	  public virtual AbstractModel SingularPronounResolverModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[SINGULAR_PRONOUN_RESOLVER_MODEL_ENTRY_NAME];
		  }
	  }

	  public virtual AbstractModel ProperNounResolverModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[PROPER_NOUN_RESOLVER_MODEL_ENTRY_NAME];
		  }
	  }

	  public virtual AbstractModel SimModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[SIM_MODEL_ENTRY_NAME];
		  }
	  }

	  public virtual AbstractModel PluralPronounResolverModel
	  {
		  get
		  {
			return (AbstractModel) artifactMap[PLURAL_PRONOUN_RESOLVER_MODEL_ENTRY_NAME];
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
	  public static void Main(string[] args)
	  {

		if (args.Length != 1)
		{
		  Console.Error.WriteLine("Usage: CorefModel projectDirectory");
		  Environment.Exit(-1);
		}

		string projectDirectory = args[0];

		CorefModel model = new CorefModel("en", projectDirectory);
		model.serialize(new FileOutputStream("coref.model"));
	  }
	}

}