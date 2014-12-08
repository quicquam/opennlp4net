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
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.IO.Reader;
using opennlp.tools.dictionary;

namespace opennlp.tools.cmdline.dictionary
{
    public class DictionaryBuilderTool : BasicCmdLineTool
	{

	  internal interface Params : DictionaryBuilderParams
	  {
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "builds a new dictionary";
		  }
	  }

	  public override string Help
	  {
		  get
		  {
			return getBasicHelp<Dictionary>(typeof(Params));
		  }
	  }

	  public override void run(string[] args)
	  {
		Params @params = validateAndParseParams<Params>(args, typeof(Params));

		Jfile dictInFile = @params.InputFile;
		Jfile dictOutFile = @params.OutputFile;
		Charset encoding = @params.Encoding;

		CmdLineUtil.checkInputFile("dictionary input file", dictInFile);
		CmdLineUtil.checkOutputFile("dictionary output file", dictOutFile);

		InputStreamReader @in = null;
		OutputStream @out = null;
		try
		{
		  @in = new InputStreamReader(new FileInputStream(dictInFile), encoding);
		  @out = new FileOutputStream(dictOutFile);

		  Dictionary dict = Dictionary.parseOneEntryPerLine(@in);
		  dict.serialize(@out);

		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while reading training data or indexing data: " + e.Message, e);
		}
		finally
		{
		  try
		  {
			@in.close();
			@out.close();
		  }
		  catch (IOException)
		  {
			// sorry that this can fail
		  }
		}

	  }

	}

}