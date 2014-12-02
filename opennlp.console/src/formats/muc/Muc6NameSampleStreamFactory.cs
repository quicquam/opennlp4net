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
using j4n.IO.File;
using j4n.Serialization;
using opennlp.console.cmdline;
using opennlp.console.cmdline.@params;
using opennlp.console.cmdline.tokenizer;
using opennlp.console.formats.convert;
using opennlp.tools.namefind;
using opennlp.tools.tokenize;

namespace opennlp.console.formats.muc
{
    public class Muc6NameSampleStreamFactory : AbstractSampleStreamFactory<NameSample>
	{

	  internal interface Parameters : BasicFormatParams
	  {
		Jfile TokenizerModel {get;}
	      Jfile[] Data { get; set; }
	  }

	  protected internal Muc6NameSampleStreamFactory()
	  {
	  }

	    public Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public ObjectStream<NameSample> create(string[] args)
	  {

		Parameters @params = ArgumentParser.parse<Parameters>(args);

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
		StreamFactoryRegistry<NameSample>.registerFactory(typeof(NameSample), "muc6", new Muc6NameSampleStreamFactory());
	  }
	}

}