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

namespace opennlp.tools.cmdline.namefind
{


	using OptionalParameter = opennlp.tools.cmdline.ArgumentParser.OptionalParameter;
	using ParameterDescription = opennlp.tools.cmdline.ArgumentParser.ParameterDescription;
	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using NameFinderCensus90NameStream = opennlp.tools.formats.NameFinderCensus90NameStream;
	using opennlp.tools.util;
	using StringList = opennlp.tools.util.StringList;

	/// <summary>
	/// This tool helps create a loadable dictionary for the {@code NameFinder},
	/// from data collected from US Census data.
	/// <para>
	/// Data for the US Census and names can be found here for the 1990 Census:
	/// <br>
	/// <a href="http://www.census.gov/genealogy/names/names_files.html">www.census.gov</a>
	/// </para>
	/// </summary>
	public class CensusDictionaryCreatorTool : BasicCmdLineTool
	{

	  /// <summary>
	  /// Create a list of expected parameters.
	  /// </summary>
	  internal interface Parameters
	  {

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "code") @OptionalParameter(defaultValue = "en") String getLang();
		string Lang {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "charsetName") @OptionalParameter(defaultValue="UTF-8") String getEncoding();
		string Encoding {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "censusDict") String getCensusData();
		string CensusData {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterDescription(valueName = "dict") String getDict();
		string Dict {get;}
	  }

	  public override string ShortDescription
	  {
		  get
		  {
			return "Converts 1990 US Census names into a dictionary";
		  }
	  }


	  public override string Help
	  {
		  get
		  {
			return getBasicHelp(typeof(Parameters));
		  }
	  }

	  /// <summary>
	  /// Creates a dictionary.
	  /// </summary>
	  /// <param name="sampleStream"> stream of samples. </param>
	  /// <returns> a {@code Dictionary} class containing the name dictionary
	  ///    built from the input file. </returns>
	  /// <exception cref="IOException"> IOException </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.tools.dictionary.Dictionary createDictionary(opennlp.tools.util.ObjectStream<opennlp.tools.util.StringList> sampleStream) throws java.io.IOException
	  public static Dictionary createDictionary(ObjectStream<StringList> sampleStream)
	  {

		Dictionary mNameDictionary = new Dictionary(true);
		StringList entry;

		entry = sampleStream.read();
		while (entry != null)
		{
		  if (!mNameDictionary.contains(entry))
		  {
			mNameDictionary.put(entry);
		  }
		  entry = sampleStream.read();
		}

		return mNameDictionary;
	  }

	  public override void run(string[] args)
	  {
		Parameters @params = validateAndParseParams(args, typeof(Parameters));

		File testData = new File(@params.CensusData);
		File dictOutFile = new File(@params.Dict);

		CmdLineUtil.checkInputFile("Name data", testData);
		CmdLineUtil.checkOutputFile("Dictionary file", dictOutFile);

		FileInputStream sampleDataIn = CmdLineUtil.openInFile(testData);
		ObjectStream<StringList> sampleStream = new NameFinderCensus90NameStream(sampleDataIn, Charset.forName(@params.Encoding));

		Dictionary mDictionary;
		try
		{
		  Console.WriteLine("Creating Dictionary...");
		  mDictionary = createDictionary(sampleStream);
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while reading training data or indexing data: " + e.Message, e);
		}
		finally
		{
		  try
		  {
			sampleStream.close();
		  }
		  catch (IOException)
		  {
			// sorry this can fail..
		  }
		}

		Console.WriteLine("Saving Dictionary...");

		OutputStream @out = null;

		try
		{
		  @out = new FileOutputStream(dictOutFile);
		  mDictionary.serialize(@out);
		}
		catch (IOException e)
		{
		  throw new TerminateToolException(-1, "IO error while writing dictionary file: " + e.Message, e);
		}
		finally
		{
		  if (@out != null)
		  {
			try
			{
			  @out.close();
			}
			catch (IOException e)
			{
			  // file might be damaged
			  throw new TerminateToolException(-1, "Attention: Failed to correctly write dictionary:" + e.Message, e);
			}
		  }
		}
	  }
	}

}