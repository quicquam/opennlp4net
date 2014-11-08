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
using System.Text.RegularExpressions;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.IO.Reader;
using j4n.IO.Writer;


namespace opennlp.tools.postag
{


	using opennlp.tools.util;

	/// <summary>
	/// Class for writing a pos-tag-dictionary to a file.
	/// </summary>
	[Obsolete]
	public class POSDictionaryWriter
	{

	  private Writer dictFile;
	  private IDictionary<string, HashSet<string>> dictionary;
	  private CountedSet<string> wordCounts;
	  private string newline = Environment.NewLine;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public POSDictionaryWriter(String file, String encoding) throws java.io.IOException
	  public POSDictionaryWriter(string file, string encoding)
	  {
		if (encoding != null)
		{
		  dictFile = new OutputStreamWriter(new FileOutputStream(file),encoding);
		}
		else
		{
		  dictFile = new FileWriter(file);
		}
		dictionary = new Dictionary<string, HashSet<string>>();
		wordCounts = new CountedSet<string>();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public POSDictionaryWriter(String file) throws java.io.IOException
	  public POSDictionaryWriter(string file) : this(file,null)
	  {
	  }

	  public virtual void addEntry(string word, string tag)
	  {
		HashSet<string> tags = dictionary[word];
		if (tags == null)
		{
		  tags = new HashSet<string>();
		  dictionary[word] = tags;
		}
		tags.Add(tag);
		wordCounts.add(word);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write() throws java.io.IOException
	  public virtual void write()
	  {
		write(5);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(int cutoff) throws java.io.IOException
	  public virtual void write(int cutoff)
	  {
		for (IEnumerator<string> wi = wordCounts.GetEnumerator(); wi.MoveNext();)
		{
		  string word = wi.Current;
		  if (wordCounts.getCount(word) >= cutoff)
		  {
			dictFile.write(word);
			HashSet<string> tags = dictionary[word];
			for (IEnumerator<string> ti = tags.GetEnumerator(); ti.MoveNext();)
			{
			  dictFile.write(" ");
			  dictFile.write(ti.Current);
			}
			dictFile.write(newline);
		  }
		}
		dictFile.close();
	  }

	  private static void usage()
	  {
		Console.Error.WriteLine("Usage: POSDictionaryWriter [-encoding encoding] dictionary tag_files");
		Environment.Exit(1);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
	  public static void Main(string[] args)
	  {
		if (args.Length == 0)
		{
		  usage();
		}
		int ai = 0;
		string encoding = null;
		if (args[ai].StartsWith("-encoding", StringComparison.Ordinal))
		{
		  if (ai + 1 >= args.Length)
		  {
			usage();
		  }
		  else
		  {
			encoding = args[ai + 1];
			ai += 2;
		  }
		}
		string dictionaryFile = args[ai++];
		POSDictionaryWriter dict = new POSDictionaryWriter(dictionaryFile,encoding);
		for (int fi = ai;fi < args.Length;fi++)
		{
		  BufferedReader @in;
		  if (encoding == null)
		  {
			@in = new BufferedReader(new FileReader(args[fi]));
		  }
		  else
		  {
			@in = new BufferedReader(new InputStreamReader(new FileInputStream(args[fi]),encoding));
		  }
		  for (string line = @in.readLine();line != null; line = @in.readLine())
		  {
			if (!line.Equals(""))
			{
			  string[] parts = Regex.Split(line,"\\s+");
			  for (int pi = 0;pi < parts.Length;pi++)
			  {
				int index = parts[pi].LastIndexOf('_');
				string word = parts[pi].Substring(0,index);
				string tag = parts[pi].Substring(index + 1);
				dict.addEntry(word,tag);
			  }
			}
		  }
		}
		dict.write();
	  }
	}

}