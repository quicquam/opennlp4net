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
using opennlp.nonjava.helperclasses;


namespace opennlp.tools.doccat
{


	using WhitespaceTokenizer = opennlp.tools.tokenize.WhitespaceTokenizer;

	/// <summary>
	/// Class which holds a classified document and its category.
	/// </summary>
	public class DocumentSample
	{

	  private readonly string category;
	  private readonly IList<string> text;

	  public DocumentSample(string category, string text) : this(category, WhitespaceTokenizer.INSTANCE.tokenize(text))
	  {
	  }

	  public DocumentSample(string category, string[] text)
	  {
		if (category == null)
		{
		  throw new System.ArgumentException("category must not be null");
		}
		if (text == null)
		{
		  throw new System.ArgumentException("text must not be null");
		}

		this.category = category;
	    this.text = text.ToList();
	  }

	  public virtual string Category
	  {
		  get
		  {
			return category;
		  }
	  }

	  public virtual string[] Text
	  {
		  get
		  {
			return text.ToArray();
		  }
	  }

	  public override string ToString()
	  {

		StringBuilder sampleString = new StringBuilder();

		sampleString.Append(category).Append('\t');

		foreach (string s in text)
		{
		  sampleString.Append(s).Append(' ');
		}

		if (sampleString.Length > 0)
		{
		  // remove last space
		  sampleString.Length = sampleString.Length - 1;
		}

		return sampleString.ToString();
	  }

	  public override bool Equals(object obj)
	  {
		if (this == obj)
		{
		  return true;
		}
		else if (obj is DocumentSample)
		{
		  DocumentSample a = (DocumentSample) obj;

		  return Category.Equals(a.Category) && Equals(Text, a.Text);
		}
		else
		{
		  return false;
		}
	  }
	}

}