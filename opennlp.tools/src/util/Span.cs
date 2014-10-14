using System;
using System.Collections.Generic;
using System.Linq;
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


namespace opennlp.tools.util
{

	/// <summary>
	/// Class for storing start and end integer offsets.
	/// 
	/// </summary>
	public class Span : IComparable<Span>
	{

	  private readonly int start;
	  private readonly int end;

	  private readonly string type;

	  /// <summary>
	  /// Initializes a new Span Object.
	  /// </summary>
	  /// <param name="s"> start of span. </param>
	  /// <param name="e"> end of span, which is +1 more than the last element in the span. </param>
	  /// <param name="type"> the type of the span </param>
	  public Span(int s, int e, string type)
	  {

		if (s < 0)
		{
		  throw new System.ArgumentException("start index must be zero or greater: " + s);
		}
		if (e < 0)
		{
		  throw new System.ArgumentException("end index must be zero or greater: " + e);
		}
		if (s > e)
		{
		  throw new System.ArgumentException("start index must not be larger than end index: " + "start=" + s + ", end=" + e);
		}

		start = s;
		end = e;
		this.type = type;
	  }

	  /// <summary>
	  /// Initializes a new Span Object.
	  /// </summary>
	  /// <param name="s"> start of span. </param>
	  /// <param name="e"> end of span. </param>
	  public Span(int s, int e) : this(s, e, null)
	  {
	  }

	  /// <summary>
	  /// Initializes a new Span object with an existing Span
	  /// which is shifted by an offset.
	  /// </summary>
	  /// <param name="span"> </param>
	  /// <param name="offset"> </param>
	  public Span(Span span, int offset) : this(span.start + offset, span.end + offset, span.Type)
	  {
	  }

	  /// <summary>
	  /// Return the start of a span.
	  /// </summary>
	  /// <returns> the start of a span.
	  ///  </returns>
	  public virtual int Start
	  {
		  get
		  {
			return start;
		  }
	  }

	  /// <summary>
	  /// Return the end of a span.
	  /// 
	  /// Note: that the returned index is one past the
	  /// actual end of the span in the text, or the first
	  /// element past the end of the span.
	  /// </summary>
	  /// <returns> the end of a span.
	  ///  </returns>
	  public virtual int End
	  {
		  get
		  {
			return end;
		  }
	  }

	  /// <summary>
	  /// Retrieves the type of the span.
	  /// </summary>
	  /// <returns> the type or null if not set </returns>
	  public virtual string Type
	  {
		  get
		  {
			return type;
		  }
	  }

	  /// <summary>
	  /// Returns the length of this span.
	  /// </summary>
	  /// <returns> the length of the span. </returns>
	  public virtual int length()
	  {
		return end - start;
	  }

	  /// <summary>
	  /// Returns true if the specified span is contained by this span.
	  /// Identical spans are considered to contain each other.
	  /// </summary>
	  /// <param name="s"> The span to compare with this span.
	  /// </param>
	  /// <returns> true is the specified span is contained by this span;
	  /// false otherwise. </returns>
	  public virtual bool contains(Span s)
	  {
		return start <= s.Start && s.End <= end;
	  }

	  /// <summary>
	  /// Returns true if the specified index is contained inside this span.
	  /// An index with the value of end is considered outside the span.
	  /// </summary>
	  /// <param name="index"> the index to test with this span.
	  /// </param>
	  /// <returns> true if the span contains this specified index;
	  /// false otherwise. </returns>
	  public virtual bool contains(int index)
	  {
		return start <= index && index < end;
	  }

	  /// <summary>
	  /// Returns true if the specified span is the begin of this span and the
	  /// specified span is contained in this span.
	  /// </summary>
	  /// <param name="s"> The span to compare with this span.
	  /// </param>
	  /// <returns> true if the specified span starts with this span and is
	  /// contained in this span; false otherwise </returns>
	  public virtual bool StartsWith(Span s)
	  {
		return Start == s.Start && contains(s);
	  }

	  /// <summary>
	  /// Returns true if the specified span intersects with this span.
	  /// </summary>
	  /// <param name="s"> The span to compare with this span.
	  /// </param>
	  /// <returns> true is the spans overlap; false otherwise. </returns>
	  public virtual bool intersects(Span s)
	  {
		int sstart = s.Start;
		//either s's start is in this or this' start is in s
		return this.contains(s) || s.contains(this) || Start <= sstart && sstart < End || sstart <= Start && Start < s.End;
	  }

	  /// <summary>
	  /// Returns true is the specified span crosses this span.
	  /// </summary>
	  /// <param name="s"> The span to compare with this span.
	  /// </param>
	  /// <returns> true is the specified span overlaps this span and contains a
	  /// non-overlapping section; false otherwise. </returns>
	  public virtual bool crosses(Span s)
	  {
		int sstart = s.Start;
		//either s's start is in this or this' start is in s
		return !this.contains(s) && !s.contains(this) && (Start <= sstart && sstart < End || sstart <= Start && Start < s.End);
	  }

	  /// <summary>
	  /// Retrieves the string covered by the current span of the specified text.
	  /// </summary>
	  /// <param name="text">
	  /// </param>
	  /// <returns> the substring covered by the current span </returns>
	  public virtual CharSequence getCoveredText(CharSequence text)
	  {
		if (End > text.length())
		{
		  throw new System.ArgumentException("The span " + ToString() + " is outside the given text which has length " + text.length() + "!");
		}

		return text.subSequence(Start, End);
	  }

	  /// <summary>
	  /// Compares the specified span to the current span.
	  /// </summary>
	  public virtual int CompareTo(Span s)
	  {
		if (Start < s.Start)
		{
		  return -1;
		}
		else if (Start == s.Start)
		{
		  if (End > s.End)
		  {
			return -1;
		  }
		  else if (End < s.End)
		  {
			return 1;
		  }
		  else
		  {
			// compare the type
			if (Type == null && s.Type == null)
			{
			  return 0;
			}
			else if (Type != null && s.Type != null)
			{
			  // use type lexicography order
			  return Type.CompareTo(s.Type);
			}
			else if (Type != null)
			{
			  return -1;
			}
			return 1;
		  }
		}
		else
		{
		  return 1;
		}
	  }

	  /// <summary>
	  /// Generates a hash code of the current span.
	  /// </summary>
	  public override int GetHashCode()
	  {
		int res = 23;
		res = res * 37 + Start;
		res = res * 37 + End;
		if (Type == null)
		{
		  res = res * 37;
		}
		else
		{
		  res = res * 37 + Type.GetHashCode();
		}

		return res;
	  }

	  /// <summary>
	  /// Checks if the specified span is equal to the current span.
	  /// </summary>
	  public override bool Equals(object o)
	  {

		bool result;

		if (o == this)
		{
		  result = true;
		}
		else if (o is Span)
		{
		  Span s = (Span) o;

		  result = (Start == s.Start) && (End == s.End) && (Type != null ? type.Equals(s.Type) : true) && (s.Type != null ? s.Type.Equals(Type) : true);
		}
		else
		{
		  result = false;
		}

		return result;
	  }

	  /// <summary>
	  /// Generates a human readable string.
	  /// </summary>
	  public override string ToString()
	  {
		StringBuilder toStringBuffer = new StringBuilder(15);
		toStringBuffer.Append("[");
		toStringBuffer.Append(Start);
		toStringBuffer.Append("..");
		toStringBuffer.Append(End);
		toStringBuffer.Append(")");
		if (Type != null)
		{
			toStringBuffer.Append(" ");
			toStringBuffer.Append(Type);
		}

		return toStringBuffer.ToString();
	  }

	  /// <summary>
	  /// Converts an array of <seealso cref="Span"/>s to an array of <seealso cref="String"/>s.
	  /// </summary>
	  /// <param name="spans"> </param>
	  /// <param name="s"> </param>
	  /// <returns> the strings </returns>
	  public static string[] spansToStrings(Span[] spans, CharSequence s)
	  {
		string[] tokens = new string[spans.Length];

		for (int si = 0, sl = spans.Length; si < sl; si++)
		{
		  tokens[si] = spans[si].getCoveredText(s).ToString();
		}

		return tokens;
	  }

	  public static string[] spansToStrings(Span[] spans, string[] tokens)
	  {
		string[] chunks = new string[spans.Length];
		StringBuilder cb = new StringBuilder();
		for (int si = 0, sl = spans.Length; si < sl; si++)
		{
		  cb.Length = 0;
		  for (int ti = spans[si].Start;ti < spans[si].End;ti++)
		  {
			cb.Append(tokens[ti]).Append(" ");
		  }
		  chunks[si] = cb.ToString().Substring(0, cb.Length - 1);
		}
		return chunks;
	  }

	    public string getCoveredText(string text)
	    {
            if (End > text.Length)
            {
                throw new System.ArgumentException("The span " + ToString() + " is outside the given text which has length " + text.Length + "!");
            }

            return text.Substring(Start, (End - Start));
        }

	    public static string[] spansToStrings(Span[] tokenizePos, string tokens)
	    {
	        return tokenizePos.Select(span => tokens.Substring(span.Start, span.length())).ToArray();
	    }
	}

}