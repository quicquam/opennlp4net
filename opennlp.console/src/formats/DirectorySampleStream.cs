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
using System.Collections.Generic;
using j4n.IO.File;
using j4n.Serialization;

namespace opennlp.console.formats
{
    /// <summary>
	/// The directory sample stream scans a directory (recursively) for plain text
	/// files and outputs each file as a String object.
	/// </summary>
	public class DirectorySampleStream : ObjectStream<Jfile>
	{

        private readonly IList<Jfile> inputDirectories;

	  private readonly bool isRecursiveScan;

	  private readonly FileFilter fileFilter;

      private Stack<Jfile> directories = new Stack<Jfile>();

      private Stack<Jfile> textFiles = new Stack<Jfile>();

      public DirectorySampleStream(Jfile[] dirs, FileFilter fileFilter, bool recursive)
	  {

		this.fileFilter = fileFilter;
		isRecursiveScan = recursive;

        IList<Jfile> inputDirectoryList = new List<Jfile>(dirs.Length);

        foreach (Jfile dir in dirs)
		{
		  if (!dir.IsDirectory)
		  {
			throw new System.ArgumentException("All passed in directories must be directories, but \"" + dir.ToString() + "\" is not!");
		  }

		  inputDirectoryList.Add(dir);
		}

		inputDirectories = inputDirectoryList;
          foreach (var inputdir in inputDirectories)
          {
              directories.Push(inputdir);
          }
	  }

      public DirectorySampleStream(Jfile dir, FileFilter fileFilter, bool recursive)
          : this(new Jfile[] { dir }, fileFilter, recursive)
	  {
	  }

      public override Jfile read()
	  {

		while (textFiles.Count == 0 && directories.Count > 0)
		{
            Jfile dir = directories.Pop();

            Jfile[] files;

		  if (fileFilter != null)
		  {
			files = dir.listFiles(fileFilter);
		  }
		  else
		  {
			files = dir.listFiles();
		  }

          foreach (Jfile file in files)
		  {
			if (file.IsFile)
			{
			  textFiles.Push(file);
			}
			else if (isRecursiveScan && file.IsDirectory)
			{
			  directories.Push(file);
			}
		  }
		}

		if (textFiles.Count > 0)
		{
		  return textFiles.Pop();
		}
		else
		{
		  return null;
		}
	  }

	  public override void reset()
	  {
		directories.Clear();
		textFiles.Clear();
        foreach (var inputdir in inputDirectories)
	      {
	          directories.Push(inputdir);
	      }

	  }

	  public override void close()
	  {
	  }
	}

}