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
using j4n.Exceptions;
using j4n.IO.File;
using j4n.IO.OutputStream;
using j4n.IO.Reader;
using j4n.Serialization;
using opennlp.tools.cmdline;

namespace opennlp.tools.formats
{


	using ArgumentParser = opennlp.tools.cmdline.ArgumentParser;
	using CmdLineUtil = opennlp.tools.cmdline.CmdLineUtil;
	using TerminateToolException = opennlp.tools.cmdline.TerminateToolException;
	using BasicFormatParams = opennlp.tools.cmdline.@params.BasicFormatParams;
	using POSSample = opennlp.tools.postag.POSSample;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ConllXPOSSampleStreamFactory : AbstractSampleStreamFactory<POSSample>
	{

	  public const string CONLLX_FORMAT = "conllx";

	  internal interface Parameters : BasicFormatParams
	  {
	      Jfile Data { get; set; }
	  }

	  public static void registerFactory()
	  {
		StreamFactoryRegistry<POSSample>.registerFactory(typeof(POSSample), CONLLX_FORMAT, new ConllXPOSSampleStreamFactory(typeof(Parameters)));
	  }

	  protected internal ConllXPOSSampleStreamFactory(Type @params)
	  {
	  }

	    public Type getParameters()
	    {
	        throw new NotImplementedException();
	    }

	    public ObjectStream<POSSample> create(string[] args)
	  {
		Parameters @params = ArgumentParser.parse<Parameters>(args);

		ObjectStream<string> lineStream;
		try
		{
		  lineStream = new PlainTextByLineStream(new InputStreamReader(CmdLineUtil.openInFile(@params.Data), "UTF-8"));
		  //Console.Out = new PrintStream(System.out, true, "UTF-8");

		  return new ConllXPOSSampleStream(lineStream);
		}
		catch (UnsupportedEncodingException e)
		{
		  // this shouldn't happen
		  throw new TerminateToolException(-1, "UTF-8 encoding is not supported: " + e.Message, e);
		}
	  }
	}

}