using System;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
using j4n.Interfaces;

using j4n.IO.File;
using j4n.IO.OutputStream;
using j4n.IO.Writer;

namespace opennlp.model
{


	using BinaryGISModelWriter = opennlp.maxent.io.BinaryGISModelWriter;
	using BinaryQNModelWriter = opennlp.maxent.io.BinaryQNModelWriter;
	using PlainTextGISModelWriter = opennlp.maxent.io.PlainTextGISModelWriter;
	using ModelType = opennlp.model.AbstractModel.ModelTypeEnum;
	using BinaryPerceptronModelWriter = opennlp.perceptron.BinaryPerceptronModelWriter;
	using PlainTextPerceptronModelWriter = opennlp.perceptron.PlainTextPerceptronModelWriter;

	public class GenericModelWriter : AbstractModelWriter
	{

	  private AbstractModelWriter delegateWriter;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GenericModelWriter(AbstractModel model, java.io.File file) throws java.io.IOException
	  public GenericModelWriter(AbstractModel model, Jfile file)
	  {
		string filename = file.Name;
		OutputStream os;
		// handle the zipped/not zipped distinction
		if (filename.EndsWith(".gz", StringComparison.Ordinal))
		{
		  os = new GZIPOutputStream(new FileOutputStream(file));
		  filename = filename.Substring(0,filename.Length - 3);
		}
		else
		{
		  os = new FileOutputStream(file);
		}

		// handle the different formats
		if (filename.EndsWith(".bin", StringComparison.Ordinal))
		{
		  init(model,new DataOutputStream(os));
		}
		else // filename ends with ".txt"
		{
		  init(model,new BufferedWriter(new OutputStreamWriter(os)));
		}
	  }

	  public GenericModelWriter(AbstractModel model, DataOutputStream dos)
	  {
		init(model,dos);
	  }

	  private void init(AbstractModel model, DataOutputStream dos)
	  {
		if (model.ModelType == ModelType.Perceptron)
		{
		  delegateWriter = new BinaryPerceptronModelWriter(model,dos);
		}
		else if (model.ModelType == ModelType.Maxent)
		{
		  delegateWriter = new BinaryGISModelWriter(model,dos);
		}
		else if (model.ModelType == ModelType.MaxentQn)
		{
			delegateWriter = new BinaryQNModelWriter(model,dos);
		}
	  }

	  private void init(AbstractModel model, BufferedWriter bw)
	  {
		if (model.ModelType == ModelType.Perceptron)
		{
		  delegateWriter = new PlainTextPerceptronModelWriter(model,bw);
		}
		else if (model.ModelType == ModelType.Maxent)
		{
		  delegateWriter = new PlainTextGISModelWriter(model,bw);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
	  public override void close()
	  {
		delegateWriter.close();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void persist() throws java.io.IOException
	  public override void persist()
	  {
		delegateWriter.persist();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeDouble(double d) throws java.io.IOException
	  public override void writeDouble(double d)
	  {
		delegateWriter.writeDouble(d);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeInt(int i) throws java.io.IOException
	  public override void writeInt(int i)
	  {
		delegateWriter.writeInt(i);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeUTF(String s) throws java.io.IOException
	  public override void writeUTF(string s)
	  {
		delegateWriter.writeUTF(s);
	  }
	}

}