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
using System.IO;
using System.Xml;
using System.Xml.Linq;
using j4n.Exceptions;
using j4n.IO.InputStream;


namespace opennlp.tools.util.featuregen
{
    using Dictionary = opennlp.tools.dictionary.Dictionary;
    using ExtensionLoader = opennlp.tools.util.ext.ExtensionLoader<AdaptiveFeatureGenerator>;

    /// <summary>
    /// Creates a set of feature generators based on a provided XML descriptor.
    /// 
    /// Example of an XML descriptor:
    /// 
    /// <generators>
    ///   <charngram min = "2" max = "5"/>
    ///   <definition/>
    ///   <cache>
    ///     <window prevLength = "3" nextLength = "3">
    ///       <generators>
    ///         <prevmap/>
    ///         <sentence/>
    ///         <tokenclass/>
    ///         <tokenpattern/>
    ///       </generators>
    ///     </window>
    ///   </cache>
    /// </generators>
    /// 
    /// Each XML XmlElement is mapped to a <seealso cref="GeneratorFactory.XmlFeatureGeneratorFactory"/> which
    /// is responsible to process the XmlElement and create the specified
    /// <seealso cref="AdaptiveFeatureGenerator"/>. XmlElements can contain other
    /// XmlElements in this case it is the responsibility of the mapped factory to process
    /// the child XmlElements correctly. In some factories this leads to recursive
    /// calls the 
    /// <seealso cref="GeneratorFactory.XmlFeatureGeneratorFactory#create(XmlElement, FeatureGeneratorResourceProvider)"/>
    /// method.
    /// 
    /// In the example above the generators XmlElement is mapped to the
    /// <seealso cref="GeneratorFactory.AggregatedFeatureGeneratorFactory"/> which then
    /// creates all the aggregated <seealso cref="AdaptiveFeatureGenerator"/>s to
    /// accomplish this it evaluates the mapping with the same mechanism
    /// and gives the child XmlElement to the corresponding factories. All
    /// created generators are added to a new instance of the
    /// <seealso cref="AggregatedFeatureGenerator"/> which is then returned.
    /// </summary>
    public class GeneratorFactory
    {
        /// <summary>
        /// The <seealso cref="XmlFeatureGeneratorFactory"/> is responsible to construct
        /// an <seealso cref="AdaptiveFeatureGenerator"/> from an given XML <seealso cref="XmlElement"/>
        /// which contains all necessary configuration if any.
        /// </summary>
        internal interface XmlFeatureGeneratorFactory
        {
            /// <summary>
            /// Creates an <seealso cref="AdaptiveFeatureGenerator"/> from a the describing
            /// XML XmlElement.
            /// </summary>
            /// <param name="generatorXmlElement"> the XmlElement which contains the configuration </param>
            /// <param name="resourceManager"> the resource manager which could be used
            ///     to access referenced resources
            /// </param>
            /// <returns> the configured <seealso cref="AdaptiveFeatureGenerator"/> </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException;
            AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager);
        }

        /// <seealso cref= AggregatedFeatureGenerator </seealso>
        internal class AggregatedFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                ICollection<AdaptiveFeatureGenerator> aggregatedGenerators = new LinkedList<AdaptiveFeatureGenerator>();

                XmlNodeList childNodes = generatorXmlElement.ChildNodes;

                for (int i = 0; i < childNodes.Count; i++)
                {
                    XmlNode childNode = childNodes[i];

                    if (childNode is XmlElement)
                    {
                        XmlElement aggregatedGeneratorXmlElement = (XmlElement) childNode;

                        aggregatedGenerators.Add(GeneratorFactory.createGenerator(aggregatedGeneratorXmlElement,
                            resourceManager));
                    }
                }

                return new AggregatedFeatureGenerator(aggregatedGenerators);
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["generators"] = new AggregatedFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= CachedFeatureGenerator </seealso>
        internal class CachedFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            internal CachedFeatureGeneratorFactory()
            {
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                XmlElement cachedGeneratorXmlElement = null;

                XmlNodeList kids = generatorXmlElement.ChildNodes;

                for (int i = 0; i < kids.Count; i++)
                {
                    XmlNode childNode = kids[i];

                    if (childNode is XmlElement)
                    {
                        cachedGeneratorXmlElement = (XmlElement) childNode;
                        break;
                    }
                }

                if (cachedGeneratorXmlElement == null)
                {
                    throw new InvalidFormatException("Could not find containing generator XmlElement!");
                }

                AdaptiveFeatureGenerator cachedGenerator = GeneratorFactory.createGenerator(cachedGeneratorXmlElement,
                    resourceManager);

                return new CachedFeatureGenerator(cachedGenerator);
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["cache"] = new CachedFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= CharacterNgramFeatureGenerator </seealso>
        internal class CharacterNgramFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                string minString = generatorXmlElement.GetAttribute("min");

                int min;

                try
                {
                    min = Convert.ToInt32(minString);
                }
                catch (NumberFormatException e)
                {
                    throw new InvalidFormatException("min attribute '" + minString + "' is not a number!", e);
                }

                string maxString = generatorXmlElement.GetAttribute("max");

                int max;

                try
                {
                    max = Convert.ToInt32(maxString);
                }
                catch (NumberFormatException e)
                {
                    throw new InvalidFormatException("max attribute '" + maxString + "' is not a number!", e);
                }

                return new CharacterNgramFeatureGenerator(min, max);
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["charngram"] = new CharacterNgramFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= DefinitionFeatureGenerator </seealso>
        internal class DefinitionFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            internal const string XmlElement_NAME = "definition";

            internal DefinitionFeatureGeneratorFactory()
            {
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                return new OutcomePriorFeatureGenerator();
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap[XmlElement_NAME] = new DefinitionFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= DictionaryFeatureGenerator </seealso>
        internal class DictionaryFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                string dictResourceKey = generatorXmlElement.GetAttribute("dict");

                object dictResource = resourceManager.getResource(dictResourceKey);

                if (!(dictResource is Dictionary))
                {
                    throw new InvalidFormatException("No dictionary resource for key: " + dictResourceKey);
                }

                string prefix = generatorXmlElement.GetAttribute("prefix");

                return new DictionaryFeatureGenerator(prefix, (Dictionary) dictResource);
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["dictionary"] = new DictionaryFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= PreviousMapFeatureGenerator </seealso>
        internal class PreviousMapFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                return new PreviousMapFeatureGenerator();
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["prevmap"] = new PreviousMapFeatureGeneratorFactory();
            }
        }

        // TODO: Add parameters ... 

        /// <seealso cref= SentenceFeatureGenerator </seealso>
        internal class SentenceFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                string beginFeatureString = generatorXmlElement.GetAttribute("begin");

                bool beginFeature = true;
                if (beginFeatureString.Length != 0)
                {
                    beginFeature = Convert.ToBoolean(beginFeatureString);
                }

                string endFeatureString = generatorXmlElement.GetAttribute("end");
                bool endFeature = true;
                if (endFeatureString.Length != 0)
                {
                    endFeature = Convert.ToBoolean(endFeatureString);
                }

                return new SentenceFeatureGenerator(beginFeature, endFeature);
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["sentence"] = new SentenceFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= TokenClassFeatureGenerator </seealso>
        internal class TokenClassFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                // TODO: Make it configurable ...
                return new TokenClassFeatureGenerator(true);
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["tokenclass"] = new TokenClassFeatureGeneratorFactory();
            }
        }

        internal class TokenFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                return new TokenFeatureGenerator();
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["token"] = new TokenFeatureGeneratorFactory();
            }
        }

        internal class BigramNameFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                return new BigramNameFeatureGenerator();
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["bigram"] = new BigramNameFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= TokenPatternFeatureGenerator </seealso>
        internal class TokenPatternFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                return new TokenPatternFeatureGenerator();
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["tokenpattern"] = new TokenPatternFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= WindowFeatureGenerator </seealso>
        internal class WindowFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                XmlElement nestedGeneratorXmlElement = null;

                XmlNodeList kids = generatorXmlElement.ChildNodes;

                for (int i = 0; i < kids.Count; i++)
                {
                    XmlNode childNode = kids[i];

                    if (childNode is XmlElement)
                    {
                        nestedGeneratorXmlElement = (XmlElement) childNode;
                        break;
                    }
                }

                if (nestedGeneratorXmlElement == null)
                {
                    throw new InvalidFormatException("window feature generator must contain" +
                                                     " an aggregator XmlElement");
                }

                AdaptiveFeatureGenerator nestedGenerator = GeneratorFactory.createGenerator(nestedGeneratorXmlElement,
                    resourceManager);

                string prevLengthString = generatorXmlElement.GetAttribute("prevLength");

                int prevLength;

                try
                {
                    prevLength = Convert.ToInt32(prevLengthString);
                }
                catch (NumberFormatException e)
                {
                    throw new InvalidFormatException(
                        "prevLength attribute '" + prevLengthString + "' is not a number!", e);
                }

                string nextLengthString = generatorXmlElement.GetAttribute("nextLength");

                int nextLength;

                try
                {
                    nextLength = Convert.ToInt32(nextLengthString);
                }
                catch (NumberFormatException e)
                {
                    throw new InvalidFormatException(
                        "nextLength attribute '" + nextLengthString + "' is not a number!", e);
                }

                return new WindowFeatureGenerator(nestedGenerator, prevLength, nextLength);
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["window"] = new WindowFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= TokenPatternFeatureGenerator </seealso>
        internal class PrefixFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                return new PrefixFeatureGenerator();
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["prefix"] = new PrefixFeatureGeneratorFactory();
            }
        }

        /// <seealso cref= TokenPatternFeatureGenerator </seealso>
        internal class SuffixFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                return new SuffixFeatureGenerator();
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["suffix"] = new SuffixFeatureGeneratorFactory();
            }
        }

        internal class CustomFeatureGeneratorFactory : XmlFeatureGeneratorFactory
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AdaptiveFeatureGenerator create(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
            public virtual AdaptiveFeatureGenerator create(XmlElement generatorXmlElement,
                FeatureGeneratorResourceProvider resourceManager)
            {
                string featureGeneratorClassName = generatorXmlElement.GetAttribute("class");

                AdaptiveFeatureGenerator generator = ExtensionLoader.instantiateExtension(featureGeneratorClassName);

                return generator;
            }

            internal static void register(IDictionary<string, XmlFeatureGeneratorFactory> factoryMap)
            {
                factoryMap["custom"] = new CustomFeatureGeneratorFactory();
            }
        }

        private static IDictionary<string, XmlFeatureGeneratorFactory> factories =
            new Dictionary<string, XmlFeatureGeneratorFactory>();

        static GeneratorFactory()
        {
            AggregatedFeatureGeneratorFactory.register(factories);
            CachedFeatureGeneratorFactory.register(factories);
            CharacterNgramFeatureGeneratorFactory.register(factories);
            DefinitionFeatureGeneratorFactory.register(factories);
            DictionaryFeatureGeneratorFactory.register(factories);
            PreviousMapFeatureGeneratorFactory.register(factories);
            SentenceFeatureGeneratorFactory.register(factories);
            TokenClassFeatureGeneratorFactory.register(factories);
            TokenFeatureGeneratorFactory.register(factories);
            BigramNameFeatureGeneratorFactory.register(factories);
            TokenPatternFeatureGeneratorFactory.register(factories);
            PrefixFeatureGeneratorFactory.register(factories);
            SuffixFeatureGeneratorFactory.register(factories);
            WindowFeatureGeneratorFactory.register(factories);
            CustomFeatureGeneratorFactory.register(factories);
        }

        /// <summary>
        /// Creates a <seealso cref="AdaptiveFeatureGenerator"/> for the provided XmlElement.
        /// To accomplish this it looks up the corresponding factory by the
        /// XmlElement tag name. The factory is then responsible for the creation
        /// of the generator from the XmlElement.
        /// </summary>
        /// <param name="generatorXmlElement"> </param>
        /// <param name="resourceManager">
        /// 
        /// @return </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static AdaptiveFeatureGenerator createGenerator(org.w3c.dom.XmlElement generatorXmlElement, FeatureGeneratorResourceProvider resourceManager) throws opennlp.tools.util.InvalidFormatException
        internal static AdaptiveFeatureGenerator createGenerator(XmlElement generatorXmlElement,
            FeatureGeneratorResourceProvider resourceManager)
        {
            string XmlElementName = generatorXmlElement.Name;

            XmlFeatureGeneratorFactory generatorFactory = factories[XmlElementName];

            if (generatorFactory == null)
            {
                throw new InvalidFormatException("Unexpected XmlElement: " + XmlElementName);
            }

            return generatorFactory.create(generatorXmlElement, resourceManager);
        }

        /// <summary>
        /// Creates an <seealso cref="AdaptiveFeatureGenerator"/> from an provided XML descriptor.
        /// 
        /// Usually this XML descriptor contains a set of nested feature generators
        /// which are then used to generate the features by one of the opennlp
        /// components.
        /// </summary>
        /// <param name="xmlDescriptorIn"> the <seealso cref="InputStream"/> from which the descriptor
        /// is read, the stream remains open and must be closed by the caller.
        /// </param>
        /// <param name="resourceManager"> the resource manager which is used to resolve resources
        /// referenced by a key in the descriptor
        /// </param>
        /// <returns> created feature generators
        /// </returns>
        /// <exception cref="IOException"> if an error occurs during reading from the descriptor
        ///     <seealso cref="InputStream"/> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static AdaptiveFeatureGenerator create(java.io.InputStream xmlDescriptorIn, FeatureGeneratorResourceProvider resourceManager) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
        public static AdaptiveFeatureGenerator create(InputStream xmlDescriptorIn,
            FeatureGeneratorResourceProvider resourceManager)
        {
            var xmlDescriptorDOM = new XmlDocument();

            xmlDescriptorDOM.Load(xmlDescriptorIn.Stream);

            XmlElement generatorXmlElement = xmlDescriptorDOM.DocumentElement;

            return createGenerator(generatorXmlElement, resourceManager);
        }
    }
}