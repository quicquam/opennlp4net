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
using opennlp.nonjava.helperclasses;
using opennlp.tools.nonjava.extensions;

namespace opennlp.tools.coref.mention
{


	using Parse = opennlp.tools.parser.Parse;
	using Parser = opennlp.tools.parser.chunking.Parser;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// This class is a wrapper for <seealso cref="opennlp.tools.parser.Parse"/> mapping it to the API specified in <seealso cref="opennlp.tools.coref.mention.Parse"/>.
	///  This allows coreference to be done on the output of the parser.
	/// </summary>
	public class DefaultParse : AbstractParse
	{

	  public static string[] NAME_TYPES = new string[] {"person", "organization", "location", "date", "time", "percentage", "money"};

      private mention.Parse parse;
	  private int sentenceNumber;
	  private static HashSet<string> entitySet = new HashSet<string>(NAME_TYPES);

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="parse"> </param>
	  /// <param name="sentenceNumber"> </param>
	  public DefaultParse(mention.Parse parse, int sentenceNumber)
	  {
		this.parse = parse;
		this.sentenceNumber = sentenceNumber;

		// Should we just maintain a parse id map !?
	  }

	  public override int SentenceNumber
	  {
		  get
		  {
			return sentenceNumber;
		  }
	  }

	  public override IList<mention.Parse> NamedEntities
	  {
		  get
		  {
              IList<mention.Parse> names = new List<mention.Parse>();
              IList<mention.Parse> kids = new List<mention.Parse>(parse.Children);
			while (kids.Count > 0)
			{
                mention.Parse p = kids.Remove(0);
			  if (entitySet.Contains(p.Type))
			  {
				names.Add(p);
			  }
			  else
			  {
                  kids.AddRange(p.Children);
			  }
			}
			return createParses(names.ToArray());
		  }
	  }

	  public override IList<opennlp.tools.coref.mention.Parse> Children
	  {
		  get
		  {
			return createParses(parse.Children.ToArray());
		  }
	  }

	  public override IList<opennlp.tools.coref.mention.Parse> SyntacticChildren
	  {
		  get
		  {
            IList<opennlp.tools.coref.mention.Parse> kids = parse.Children;
			for (int ci = 0; ci < kids.Count; ci++)
			{
			  Parse kid = kids[ci];
			  if (entitySet.Contains(kid.Type))
			  {
				kids.RemoveAt(ci);
				kids.AddRange(kid.Children);
				ci--;
			  }
			}
			return createParses(kids.ToArray());
		  }
	  }

	  public override IList<opennlp.tools.coref.mention.Parse> Tokens
	  {
		  get
		  {
              IList<mention.Parse> tokens = new List<mention.Parse>();
              IList<mention.Parse> kids = parse.Children;
			while (kids.Count > 0)
			{
			  Parse p = kids.Remove(0);
			  if (p.PosTag)
			  {
				tokens.Add(p);
			  }
			  else
			  {
				kids.AddRange(0,Arrays.asList(p.Children));
			  }
			}
			return createParses(tokens.ToArray());
		  }
	  }

	  public override string SyntacticType
	  {
		  get
		  {
			if (entitySet.Contains(parse.Type))
			{
			  return null;
			}
			else if (parse.Type.Contains("#"))
			{
			  return parse.Type.Substring(0, parse.Type.IndexOf('#'));
			}
			else
			{
			  return parse.Type;
			}
		  }
	  }

      private IList<opennlp.tools.coref.mention.Parse> createParses(mention.Parse[] parses)
	  {
		IList<opennlp.tools.coref.mention.Parse> newParses = new List<opennlp.tools.coref.mention.Parse>(parses.Length);

		for (int pi = 0,pn = parses.Length;pi < pn;pi++)
		{
		  newParses.Add(new DefaultParse(parses[pi],sentenceNumber));
		}

		return newParses;
	  }

	  public override string EntityType
	  {
		  get
		  {
			if (entitySet.Contains(parse.Type))
			{
			  return parse.Type;
			}
			else
			{
			  return null;
			}
		  }
	  }

	  public override bool ParentNAC
	  {
		  get
		  {
			Parse parent = parse.Parent;
			while (parent != null)
			{
			  if (parent.Type.Equals("NAC"))
			  {
				return true;
			  }
			  parent = parent.Parent;
			}
			return false;
		  }
	  }

	  public override opennlp.tools.coref.mention.Parse Parent
	  {
		  get
		  {
              mention.Parse parent = parse.Parent;
			if (parent == null)
			{
			  return null;
			}
			else
			{
			  return new DefaultParse(parent,sentenceNumber);
			}
		  }
	  }

	  public override bool NamedEntity
	  {
		  get
		  {
    
			// TODO: We should use here a special tag to, where
			// the type can be extracted from. Then it just depends
			// on the training data and not the values inside NAME_TYPES.
    
			if (entitySet.Contains(parse.Type))
			{
			  return true;
			}
			else
			{
			  return false;
			}
		  }
	  }

	  public override bool NounPhrase
	  {
		  get
		  {
			return parse.Type.Equals("NP") || parse.Type.StartsWith("NP#", StringComparison.Ordinal);
		  }
	  }

	  public override bool Sentence
	  {
		  get
		  {
			return parse.Type.Equals(Parser.TOP_NODE);
		  }
	  }

	  public override bool Token
	  {
		  get
		  {
			return parse.PosTag;
		  }
	  }

	  public override int EntityId
	  {
		  get
		  {
    
			string type = parse.Type;
    
			if (type.Contains("#"))
			{
			  string numberString = type.Substring(type.IndexOf('#') + 1);
			  return Convert.ToInt32(numberString);
			}
			else
			{
			  return -1;
			}
		  }
	  }

	  public override Span Span
	  {
		  get
		  {
			return parse.Span;
		  }
	  }

	  public virtual int compareTo(opennlp.tools.coref.mention.Parse p)
	  {

		if (p == this)
		{
		  return 0;
		}

		if (SentenceNumber < p.SentenceNumber)
		{
		  return -1;
		}
		else if (SentenceNumber > p.SentenceNumber)
		{
		  return 1;
		}
		else
		{

		  if (parse.Span.Start == p.Span.Start && parse.Span.End == p.Span.End)
		  {

			Console.WriteLine("Maybe incorrect measurement!");

			Stack<Parse> parents = new Stack<Parse>();




			// get parent and update distance
			// if match return distance
			// if not match do it again
		  }

		  return parse.Span.CompareTo(p.Span);
		}
	  }

	  public override string ToString()
	  {
		return parse.CoveredText;
	  }


	  public override opennlp.tools.coref.mention.Parse PreviousToken
	  {
		  get
		  {
			Parse parent = parse.Parent;
			Parse node = parse;
			int index = -1;
			//find parent with previous children
			while (parent != null && index < 0)
			{
			  index = parent.IndexOf(node) - 1;
			  if (index < 0)
			  {
				node = parent;
				parent = parent.Parent;
			  }
			}
			//find right-most child which is a token
			if (index < 0)
			{
			  return null;
			}
			else
			{
			  Parse p = parent.Children[index];
			  while (!p.PosTag)
			  {
				Parse[] kids = p.Children;
				p = kids[kids.Length - 1];
			  }
			  return new DefaultParse(p,sentenceNumber);
			}
		  }
	  }

	  public override opennlp.tools.coref.mention.Parse NextToken
	  {
		  get
		  {
              mention.Parse parent = parse.Parent;
              mention.Parse node = parse;
			int index = -1;
			//find parent with subsequent children
			while (parent != null)
			{
			  index = parent.IndexOf(node) + 1;
			  if (index == parent.ChildCount)
			  {
				node = parent;
				parent = parent.Parent;
			  }
			  else
			  {
				break;
			  }
			}
			//find left-most child which is a token
			if (parent == null)
			{
			  return null;
			}
			else
			{
			  Parse p = parent.Children[index];
			  while (!p.PosTag)
			  {
				p = p.Children[0];
			  }
			  return new DefaultParse(p,sentenceNumber);
			}
		  }
	  }

	  public override bool Equals(object o)
	  {

		bool result;

		if (o == this)
		{
		  result = true;
		}
		else if (o is DefaultParse)
		{
		  result = parse == ((DefaultParse) o).parse;
		}
		else
		{
		  result = false;
		}

		return result;
	  }

	  public override int GetHashCode()
	  {
		return parse.GetHashCode();
	  }

	  /// <summary>
	  /// Retrieves the <seealso cref="Parse"/>.
	  /// </summary>
	  /// <returns> the <seealso cref="Parse"/> </returns>
	  public virtual mention.Parse Parse
	  {
		  get
		  {
			return parse;
		  }
	  }
	}

}