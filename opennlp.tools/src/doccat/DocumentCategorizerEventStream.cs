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
using j4n.Serialization;


namespace opennlp.tools.doccat
{
    using Event = opennlp.model.Event;
    using opennlp.tools.util;
    using opennlp.tools.util;

    /// <summary>
    /// Iterator-like class for modeling document classification events.
    /// </summary>
    public class DocumentCategorizerEventStream : AbstractEventStream<DocumentSample>
    {
        private DocumentCategorizerContextGenerator mContextGenerator;

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="data"> <seealso cref="ObjectStream"/> of <seealso cref="DocumentSample"/>s
        /// </param>
        /// <param name="featureGenerators"> </param>
        public DocumentCategorizerEventStream(ObjectStream<DocumentSample> data,
            params FeatureGenerator[] featureGenerators) : base(data)
        {
            mContextGenerator = new DocumentCategorizerContextGenerator(featureGenerators);
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="samples"> <seealso cref="ObjectStream"/> of <seealso cref="DocumentSample"/>s </param>
        public DocumentCategorizerEventStream(ObjectStream<DocumentSample> samples) : base(samples)
        {
            mContextGenerator = new DocumentCategorizerContextGenerator(new BagOfWordsFeatureGenerator());
        }

        protected internal override IEnumerator<Event> createEvents(DocumentSample sample)
        {
            // commented out MJJ 07/11/2014
            throw new NotImplementedException();
            // return new IteratorAnonymousInnerClassHelper(this, sample);
        }

/*
	  private class IteratorAnonymousInnerClassHelper : IEnumerator<Event>
	  {
		  private readonly DocumentCategorizerEventStream outerInstance;

		  private opennlp.tools.doccat.DocumentSample sample;

		  public IteratorAnonymousInnerClassHelper(DocumentCategorizerEventStream outerInstance, opennlp.tools.doccat.DocumentSample sample)
		  {
			  this.outerInstance = outerInstance;
			  this.sample = sample;
			  isVirgin = true;
		  }


		  private bool isVirgin;

		  public virtual bool hasNext()
		  {
			return isVirgin;
		  }

		  public virtual Event next()
		  {

			isVirgin = false;

			return new Event(sample.Category, outerInstance.mContextGenerator.getContext(sample.Text));
		  }

		  public virtual void remove()
		  {
			throw new System.NotSupportedException();
		  }
	  }*/
    }
}