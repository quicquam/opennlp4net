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
using j4n.IO.File;
using j4n.Serialization;

namespace opennlp.tools.formats.muc
{


	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using StreamFactoryRegistry = opennlp.tools.cmdline.StreamFactoryRegistry;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using TokenizerModelLoader = opennlp.tools.cmdline.tokenizer.TokenizerModelLoader;
	using opennlp.tools.formats;
	using FileToStringSampleStream = opennlp.tools.formats.convert.FileToStringSampleStream;
	using NameSample = opennlp.tools.namefind.NameSample;
	using Tokenizer = opennlp.tools.tokenize.Tokenizer;
	using TokenizerME = opennlp.tools.tokenize.TokenizerME;
	using TokenizerModel = opennlp.tools.tokenize.TokenizerModel;
	using opennlp.tools.util;

	public class Muc6NameSampleStreamFactory : AbstractSampleStreamFactory<NameSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "modelFile") java.io.File getTokenizerModel();
		Jfile TokenizerModel {get;}
	  }

	  protected internal Muc6NameSampleStreamFactory() : base(typeof(Parameters))
	  {
	  }

	  public override ObjectStream<NameSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse(args, typeof(Parameters));

		TokenizerModel tokenizerModel = (new TokenizerModelLoader()).load(@params.TokenizerModel);
		Tokenizer tokenizer = new TokenizerME(tokenizerModel);

		ObjectStream<string> mucDocStream = new FileToStringSampleStream(new DirectorySampleStream(@params.Data, new FileFilterAnonymousInnerClassHelper(this), false), Charset.forName("UTF-8"));

		return new MucNameSampleStream(tokenizer, mucDocStream);
	  }

	  private class FileFilterAnonymousInnerClassHelper : FileFilter
	  {
		  private readonly Muc6NameSampleStreamFactory outerInstance;

		  public FileFilterAnonymousInnerClassHelper(Muc6NameSampleStreamFactory outerInstance)
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
		StreamFactoryRegistry.registerFactory(typeof(NameSample), "muc6", new Muc6NameSampleStreamFactory());
	  }
	}

}