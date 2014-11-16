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

namespace opennlp.tools.util
{


	public class CollectionObjectStream<E> : ObjectStream<E>
	{
	  private ICollection<E> collection;

	  private IEnumerator<E> iterator;

	  public CollectionObjectStream(ICollection<E> collection)
	  {
		this.collection = collection;

		reset();
	  }

	  public virtual E read()
	  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
		if (iterator.hasNext())
		{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
		  return iterator.next();
		}
		else
		{
		  return null;
		}
	  }

	  public virtual void reset()
	  {
		this.iterator = collection.GetEnumerator();
	  }

	  public virtual void close()
	  {
	  }
	}
}