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
using System.Linq;
using j4n.IO.File;
using j4n.Serialization;

namespace opennlp.tools.formats.muc
{


	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using TokenNameFinderModelLoader = opennlp.tools.cmdline.namefind.TokenNameFinderModelLoader;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using ParserModelLoader = opennlp.tools.cmdline.parser.ParserModelLoader;
	using TokenizerModelLoader = opennlp.tools.cmdline.tokenizer.TokenizerModelLoader;
	using CorefSample = opennlp.tools.coref.CorefSample;
	using opennlp.tools.formats;
	using FileToStringSampleStream = opennlp.tools.formats.convert.FileToStringSampleStream;
	using NameFinderME = opennlp.tools.namefind.NameFinderME;
	using TokenNameFinder = opennlp.tools.namefind.TokenNameFinder;
	using Parser = opennlp.tools.parser.Parser;
	using ParserFactory = opennlp.tools.parser.ParserFactory;
	using ParserModel = opennlp.tools.parser.ParserModel;
	using Tokenizer = opennlp.tools.tokenize.Tokenizer;
	using TokenizerME = opennlp.tools.tokenize.TokenizerME;
	using TokenizerModel = opennlp.tools.tokenize.TokenizerModel;
	using opennlp.tools.util;

	/// <summary>
	/// Factory creates a stream which can parse MUC 6 Coref data and outputs CorefSample
	/// objects which are enhanced with a full parse and are suitable to train the Coreference component.
	/// </summary>
	public class Muc6FullParseCorefSampleStreamFactory : AbstractSampleStreamFactory<CorefSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {

		Jfile ParserModel {get;}

        Jfile TokenizerModel { get; }

        Jfile PersonModel { get; }

        Jfile OrganizationModel { get; }

		// TODO: Add other models here !!!
	  }

	  protected internal Muc6FullParseCorefSampleStreamFactory() : base(typeof(Parameters))
	  {
	  }

	  public override ObjectStream<CorefSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		ParserModel parserModel = (new ParserModelLoader()).load(@params.ParserModel);
		Parser parser = ParserFactory.create(parserModel);

		TokenizerModel tokenizerModel = (new TokenizerModelLoader()).load(@params.TokenizerModel);
		Tokenizer tokenizer = new TokenizerME(tokenizerModel);

		ObjectStream<string> mucDocStream = new FileToStringSampleStream(new DirectorySampleStream(@params.Data, new FileFilterAnonymousInnerClassHelper(this), false), Charset.forName("UTF-8"));

		ObjectStream<RawCorefSample> rawSamples = new MucCorefSampleStream(tokenizer, mucDocStream);

		ObjectStream<RawCorefSample> parsedSamples = new FullParseCorefEnhancerStream(parser, rawSamples);


		// How to load all these nameFinder models ?! 
		// Lets make a param per model, not that nice, but ok!

        IDictionary<string, Jfile> modelFileTagMap = new Dictionary<string, Jfile>();

		modelFileTagMap["person"] = @params.PersonModel;
		modelFileTagMap["organization"] = @params.OrganizationModel;

		IList<TokenNameFinder> nameFinders = new List<TokenNameFinder>();
		IList<string> tags = new List<string>();

		foreach (KeyValuePair<string, Jfile> entry in modelFileTagMap)
		{
		  nameFinders.Add(new NameFinderME((new TokenNameFinderModelLoader()).load(entry.Value)));
		  tags.Add(entry.Key);
		}

		return new MucMentionInserterStream(new NameFinderCorefEnhancerStream(nameFinders.ToArray(), tags.ToArray(), parsedSamples));
	  }

	  private class FileFilterAnonymousInnerClassHelper : FileFilter
	  {
		  private readonly Muc6FullParseCorefSampleStreamFactory outerInstance;

		  public FileFilterAnonymousInnerClassHelper(Muc6FullParseCorefSampleStreamFactory outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }


		  public virtual bool accept(Jfile file)
		  {
			return file.Name.ToLower().EndsWith(".sgm", StringComparison.Ordinal);
		  }
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry.registerFactory(typeof(CorefSample), "muc6full", new Muc6FullParseCorefSampleStreamFactory());
	  }
	}

}