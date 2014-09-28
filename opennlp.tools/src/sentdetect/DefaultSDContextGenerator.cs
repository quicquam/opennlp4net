using System;
using System.Collections.Generic;
using System.Text;
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

namespace opennlp.tools.sentdetect
{


	using StringUtil = opennlp.tools.util.StringUtil;

	/// <summary>
	/// Generate event contexts for maxent decisions for sentence detection.
	/// 
	/// </summary>
	public class DefaultSDContextGenerator : SDContextGenerator
	{

	  /// <summary>
	  /// String buffer for generating features.
	  /// </summary>
	  protected internal StringBuilder buf;

	  /// <summary>
	  /// List for holding features as they are generated.
	  /// </summary>
	  protected internal IList<string> collectFeats;

	  private HashSet<string> inducedAbbreviations;

	  private char[] eosCharacters;

	  /// <summary>
	  /// Creates a new <code>SDContextGenerator</code> instance with
	  /// no induced abbreviations.
	  /// </summary>
	  /// <param name="eosCharacters"> </param>
	  public DefaultSDContextGenerator(char[] eosCharacters) : this(System.Linq.Enumerable.Empty<string>(), eosCharacters)
	  {
	  }

	  /// <summary>
	  /// Creates a new <code>SDContextGenerator</code> instance which uses
	  /// the set of induced abbreviations.
	  /// </summary>
	  /// <param name="inducedAbbreviations"> a <code>Set</code> of Strings
	  /// representing induced abbreviations in the training data.
	  /// Example: &quot;Mr.&quot;
	  /// </param>
	  /// <param name="eosCharacters"> </param>
	  public DefaultSDContextGenerator(HashSet<string> inducedAbbreviations, char[] eosCharacters)
	  {
		this.inducedAbbreviations = inducedAbbreviations;
		this.eosCharacters = eosCharacters;
		buf = new StringBuilder();
		collectFeats = new List<string>();
	  }

	  /* (non-Javadoc)
	   * @see opennlp.tools.sentdetect.SDContextGenerator#getContext(java.lang.StringBuffer, int)
	   */
	  public virtual string[] getContext(CharSequence sb, int position)
	  {

		/// <summary>
		/// String preceding the eos character in the eos token.
		/// </summary>
		string prefix;

		/// <summary>
		/// Space delimited token preceding token containing eos character.
		/// </summary>
		string previous;

		/// <summary>
		/// String following the eos character in the eos token.
		/// </summary>
		string suffix;

		/// <summary>
		/// Space delimited token following token containing eos character.
		/// </summary>
		string next;

		int lastIndex = sb.length() - 1;
		{ // compute space previous and space next features.
		  if (position > 0 && StringUtil.isWhitespace(sb.charAt(position - 1)))
		  {
			collectFeats.Add("sp");
		  }
		  if (position < lastIndex && StringUtil.isWhitespace(sb.charAt(position + 1)))
		  {
			collectFeats.Add("sn");
		  }
		  collectFeats.Add("eos=" + sb.charAt(position));
		}
		int prefixStart = previousSpaceIndex(sb, position);

		int c = position;
		{ ///assign prefix, stop if you run into a period though otherwise stop at space
		  while (--c > prefixStart)
		  {
			for (int eci = 0, ecl = eosCharacters.Length; eci < ecl; eci++)
			{
			  if (sb.charAt(c) == eosCharacters[eci])
			  {
				prefixStart = c;
				c++; // this gets us out of while loop.
				break;
			  }
			}
		  }
		  prefix = (new StringBuilder(sb.subSequence(prefixStart, position))).ToString().Trim();
		}
		int prevStart = previousSpaceIndex(sb, prefixStart);
		previous = (new StringBuilder(sb.subSequence(prevStart, prefixStart))).ToString().Trim();

		int suffixEnd = nextSpaceIndex(sb, position, lastIndex);
		{
		  c = position;
		  while (++c < suffixEnd)
		  {
			for (int eci = 0, ecl = eosCharacters.Length; eci < ecl; eci++)
			{
			  if (sb.charAt(c) == eosCharacters[eci])
			  {
				suffixEnd = c;
				c--; // this gets us out of while loop.
				break;
			  }
			}
		  }
		}
		int nextEnd = nextSpaceIndex(sb, suffixEnd + 1, lastIndex + 1);
		if (position == lastIndex)
		{
		  suffix = "";
		  next = "";
		}
		else
		{
		  suffix = (new StringBuilder(sb.subSequence(position + 1, suffixEnd))).ToString().Trim();
		  next = (new StringBuilder(sb.subSequence(suffixEnd + 1, nextEnd))).ToString().Trim();
		}

		collectFeatures(prefix,suffix,previous,next, sb.charAt(position));

		string[] context = new string[collectFeats.Count];
		context = collectFeats.toArray(context);
		collectFeats.Clear();
		return context;
	  }

	  /// <summary>
	  /// Determines some of the features for the sentence detector and adds them to list features.
	  /// </summary>
	  /// <param name="prefix"> String preceding the eos character in the eos token. </param>
	  /// <param name="suffix"> String following the eos character in the eos token. </param>
	  /// <param name="previous"> Space delimited token preceding token containing eos character. </param>
	  /// <param name="next"> Space delimited token following token containing eos character.
	  /// </param>
	  /// @deprecated use <seealso cref="#collectFeatures(String, String, String, String, Character)"/> instead. 
	  protected internal virtual void collectFeatures(string prefix, string suffix, string previous, string next)
	  {
		collectFeatures(prefix, suffix, previous, next, null);
	  }

	  /// <summary>
	  /// Determines some of the features for the sentence detector and adds them to list features.
	  /// </summary>
	  /// <param name="prefix"> String preceding the eos character in the eos token. </param>
	  /// <param name="suffix"> String following the eos character in the eos token. </param>
	  /// <param name="previous"> Space delimited token preceding token containing eos character. </param>
	  /// <param name="next"> Space delimited token following token containing eos character. </param>
	  /// <param name="eosChar"> the EOS character been analyzed </param>
	  protected internal virtual void collectFeatures(string prefix, string suffix, string previous, string next, char? eosChar)
	  {
		buf.Append("x=");
		buf.Append(prefix);
		collectFeats.Add(buf.ToString());
		buf.Length = 0;
		if (!prefix.Equals(""))
		{
		  collectFeats.Add(Convert.ToString(prefix.Length));
		  if (isFirstUpper(prefix))
		  {
			collectFeats.Add("xcap");
		  }
		  if (eosChar != null && inducedAbbreviations.Contains(prefix + eosChar))
		  {
			collectFeats.Add("xabbrev");
		  }
		}

		buf.Append("v=");
		buf.Append(previous);
		collectFeats.Add(buf.ToString());
		buf.Length = 0;
		if (!previous.Equals(""))
		{
		  if (isFirstUpper(previous))
		  {
			collectFeats.Add("vcap");
		  }
		  if (inducedAbbreviations.Contains(previous))
		  {
			collectFeats.Add("vabbrev");
		  }
		}

		buf.Append("s=");
		buf.Append(suffix);
		collectFeats.Add(buf.ToString());
		buf.Length = 0;
		if (!suffix.Equals(""))
		{
		  if (isFirstUpper(suffix))
		  {
			collectFeats.Add("scap");
		  }
		  if (inducedAbbreviations.Contains(suffix))
		  {
			collectFeats.Add("sabbrev");
		  }
		}

		buf.Append("n=");
		buf.Append(next);
		collectFeats.Add(buf.ToString());
		buf.Length = 0;
		if (!next.Equals(""))
		{
		  if (isFirstUpper(next))
		  {
			collectFeats.Add("ncap");
		  }
		  if (inducedAbbreviations.Contains(next))
		  {
			collectFeats.Add("nabbrev");
		  }
		}
	  }

	  private static bool isFirstUpper(string s)
	  {
		return char.IsUpper(s[0]);
	  }

	  /// <summary>
	  /// Finds the index of the nearest space before a specified index which is not itself preceded by a space.
	  /// </summary>
	  /// <param name="sb">   The string buffer which contains the text being examined. </param>
	  /// <param name="seek"> The index to begin searching from. </param>
	  /// <returns> The index which contains the nearest space. </returns>
	  private static int previousSpaceIndex(CharSequence sb, int seek)
	  {
		seek--;
		while (seek > 0 && !StringUtil.isWhitespace(sb.charAt(seek)))
		{
		  seek--;
		}
		if (seek > 0 && StringUtil.isWhitespace(sb.charAt(seek)))
		{
		  while (seek > 0 && StringUtil.isWhitespace(sb.charAt(seek - 1)))
		  {
			seek--;
		  }
		  return seek;
		}
		return 0;
	  }

	  /// <summary>
	  /// Finds the index of the nearest space after a specified index.
	  /// </summary>
	  /// <param name="sb"> The string buffer which contains the text being examined. </param>
	  /// <param name="seek"> The index to begin searching from. </param>
	  /// <param name="lastIndex"> The highest index of the StringBuffer sb. </param>
	  /// <returns> The index which contains the nearest space. </returns>
	  private static int nextSpaceIndex(CharSequence sb, int seek, int lastIndex)
	  {
		seek++;
		char c;
		while (seek < lastIndex)
		{
		  c = sb.charAt(seek);
		  if (StringUtil.isWhitespace(c))
		  {
			while (sb.length() > seek + 1 && StringUtil.isWhitespace(sb.charAt(seek + 1)))
			{
			  seek++;
			}
			return seek;
		  }
		  seek++;
		}
		return lastIndex;
	  }
	}

}