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

using System;
using System.Collections.Generic;
using System.Linq;
using j4n.IO.File;
using opennlp.tools.cmdline;
using opennlp.tools.cmdline.namefind;
using opennlp.tools.cmdline.@params;
using opennlp.tools.cmdline.parser;
using opennlp.tools.cmdline.tokenizer;
using opennlp.tools.coref;
using opennlp.tools.formats.convert;
using opennlp.tools.namefind;
using opennlp.tools.parser;
using opennlp.tools.tokenize;
using opennlp.tools.util;

namespace opennlp.tools.formats.muc
{
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
	      Jfile[] Data { get; set; }

	      // TODO: Add other models here !!!
	  }

	  protected internal Muc6FullParseCorefSampleStreamFactory()
	  {
	  }

	    public Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public ObjectStream<CorefSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse<Parameters>(args);

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
		StreamFactoryRegistry<CorefSample>.registerFactory(typeof(CorefSample), "muc6full", new Muc6FullParseCorefSampleStreamFactory());
	  }
	}

}