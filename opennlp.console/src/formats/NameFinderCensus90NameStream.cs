using System;
/*
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  under the License.
 */
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.Serialization;

namespace opennlp.tools.formats
{


	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
	using StringList = opennlp.tools.util.StringList;
	using StringUtil = opennlp.tools.util.StringUtil;

	/// <summary>
	/// This class helps to read the US Census data from the files to build a
	/// StringList for each dictionary entry in the name-finder dictionary.
	/// The entries in the source file are as follows:
	/// <para>
	///      SMITH          1.006  1.006      1
	/// </para>
	/// <para>
	/// <ul>
	/// <li>The first field is the name (in ALL CAPS).
	/// <li>The next field is a frequency in percent.
	/// <li>The next is a cumulative frequency in percent.
	/// <li>The last is a ranking.
	/// </ul>
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class NameFinderCensus90NameStream : ObjectStream<StringList>
	{

	  private readonly Locale locale;
	  private readonly Charset encoding;
	  private readonly ObjectStream<string> lineStream;

	  /// <summary>
	  /// This constructor takes an ObjectStream and initializes the class to handle
	  /// the stream.
	  /// </summary>
	  /// <param name="lineStream">  an <code>ObjectSteam<String></code> that represents the
	  ///                    input file to be attached to this class. </param>
	  public NameFinderCensus90NameStream(ObjectStream<string> lineStream)
	  {
		this.locale = new Locale("en"); // locale is English
		this.encoding = Charset.defaultCharset();
		// todo how do we find the encoding for an already open ObjectStream() ?
		this.lineStream = lineStream;
	  }

	  /// <summary>
	  /// This constructor takes an <code>InputStream</code> and a <code>Charset</code>
	  /// and opens an associated stream object with the specified encoding specified.
	  /// </summary>
	  /// <param name="in">  an <code>InputStream</code> for the input file. </param>
	  /// <param name="encoding">  the <code>Charset</code> to apply to the input stream. </param>
	  public NameFinderCensus90NameStream(InputStream @in, Charset encoding)
	  {
		this.locale = new Locale("en"); // locale is English
		this.encoding = encoding;
		this.lineStream = new PlainTextByLineStream(@in, this.encoding);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.util.StringList read() throws java.io.IOException
	  public virtual StringList read()
	  {
		string line = lineStream.read();
		StringList name = null;

		if ((line != null) && (!StringUtil.isEmpty(line)))
		{
		  string name2;
		  // find the location of the name separator in the line of data.
		  int pos = line.IndexOf(' ');
		  if ((pos != -1))
		  {
			string parsed = line.Substring(0, pos);
			// the data is in ALL CAPS ... so the easiest way is to convert
			// back to standard mixed case.
			if ((parsed.Length > 2) && (parsed.StartsWith("MC", StringComparison.Ordinal)))
			{
			  name2 = parsed.Substring(0,1).ToUpper(locale) + parsed.Substring(1, 1).ToLower(locale) + parsed.Substring(2, 1).ToUpper(locale) + parsed.Substring(3).ToLower(locale);
			}
			else
			{
			  name2 = parsed.Substring(0,1).ToUpper(locale) + parsed.Substring(1).ToLower(locale);
			}
			name = new StringList(new string[]{name2});
		  }
		}

		return name;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws java.io.IOException, UnsupportedOperationException
	  public virtual void reset()
	  {
		lineStream.reset();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	  public virtual void close()
	  {
		lineStream.close();
	  }

	}

}