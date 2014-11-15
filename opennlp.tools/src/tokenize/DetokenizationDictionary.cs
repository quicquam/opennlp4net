using System.Collections;
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
using j4n.Interfaces;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;

namespace opennlp.tools.tokenize
{
    using Attributes = opennlp.tools.dictionary.serializer.Attributes;
    using DictionarySerializer = opennlp.tools.dictionary.serializer.DictionarySerializer;
    using Entry = opennlp.tools.dictionary.serializer.Entry;
    using EntryInserter = opennlp.tools.dictionary.serializer.EntryInserter;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using StringList = opennlp.tools.util.StringList;

    public class DetokenizationDictionary
    {
        public sealed class Operation
        {
            /// <summary>
            /// Attaches the token to the token on the right side.
            /// </summary>
            public static readonly Operation MOVE_RIGHT = new Operation("MOVE_RIGHT", InnerEnum.MOVE_RIGHT);

            /// <summary>
            /// Attaches the token to the token on the left side. 
            /// </summary>
            public static readonly Operation MOVE_LEFT = new Operation("MOVE_LEFT", InnerEnum.MOVE_LEFT);

            /// <summary>
            /// Attaches the token to the token on the left and right sides.
            /// </summary>
            public static readonly Operation MOVE_BOTH = new Operation("MOVE_BOTH", InnerEnum.MOVE_BOTH);

            /// <summary>
            /// Attaches the token token to the right token on first occurrence, and
            /// to the token on the left side on the second occurrence. 
            /// </summary>
            public static readonly Operation RIGHT_LEFT_MATCHING = new Operation("RIGHT_LEFT_MATCHING",
                InnerEnum.RIGHT_LEFT_MATCHING);

            private static readonly IList<Operation> valueList = new List<Operation>();

            static Operation()
            {
                valueList.Add(MOVE_RIGHT);
                valueList.Add(MOVE_LEFT);
                valueList.Add(MOVE_BOTH);
                valueList.Add(RIGHT_LEFT_MATCHING);
            }

            public enum InnerEnum
            {
                MOVE_RIGHT,
                MOVE_LEFT,
                MOVE_BOTH,
                RIGHT_LEFT_MATCHING
            }

            private readonly string nameValue;
            private readonly int ordinalValue;
            private readonly InnerEnum innerEnumValue;
            private static int nextOrdinal = 0;

            private Operation(string moveRight, InnerEnum innerEnum)
            {
                throw new System.NotImplementedException();
            }

            public static Operation parse(string operation)
            {
                if (MOVE_RIGHT.ToString().Equals(operation))
                {
                    return MOVE_RIGHT;
                }
                else if (MOVE_LEFT.ToString().Equals(operation))
                {
                    return MOVE_LEFT;
                }
                else if (MOVE_BOTH.ToString().Equals(operation))
                {
                    return MOVE_BOTH;
                }
                else if (RIGHT_LEFT_MATCHING.ToString().Equals(operation))
                {
                    return RIGHT_LEFT_MATCHING;
                }
                else
                {
                    return null;
                }
            }

            public static IList<Operation> values()
            {
                return valueList;
            }

            public InnerEnum InnerEnumValue()
            {
                return innerEnumValue;
            }

            public int ordinal()
            {
                return ordinalValue;
            }

            public override string ToString()
            {
                return nameValue;
            }

            public static Operation valueOf(string name)
            {
                foreach (Operation enumInstance in Operation.values())
                {
                    if (enumInstance.nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
        }

        private readonly IDictionary<string, DetokenizationDictionary.Operation> operationTable =
            new Dictionary<string, DetokenizationDictionary.Operation>();

        /// <summary>
        /// Initializes the current instance. 
        /// </summary>
        /// <param name="tokens"> </param>
        /// <param name="operations"> </param>
        public DetokenizationDictionary(string[] tokens, DetokenizationDictionary.Operation[] operations)
        {
            if (tokens.Length != operations.Length)
            {
                throw new System.ArgumentException("tokens and ops must have the same length: tokens=" + tokens.Length +
                                                   ", operations=" + operations.Length + "!");
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                DetokenizationDictionary.Operation operation = operations[i];

                if (token == null)
                {
                    throw new System.ArgumentException("token at index " + i + " must not be null!");
                }

                if (operation == null)
                {
                    throw new System.ArgumentException("operation at index " + i + " must not be null!");
                }

                operationTable[token] = operation;
            }
        }

        public DetokenizationDictionary(InputStream @in)
        {
            DictionarySerializer.create(@in, new EntryInserterAnonymousInnerClassHelper(this));
        }

        public DetokenizationDictionary(FileInputStream tokens)
        {
            throw new System.NotImplementedException();
        }

        public class EntryInserterAnonymousInnerClassHelper : EntryInserter
        {
            private readonly DetokenizationDictionary outerInstance;

            public EntryInserterAnonymousInnerClassHelper(DetokenizationDictionary outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual void insert(Entry entry)
            {
                string operationString = entry.Attributes.getValue("operation");

                StringList word = entry.Tokens;

                if (word.size() != 1)
                {
                    throw new InvalidFormatException("Each entry must have exactly one token! " + word);
                }

                // parse operation
                Operation operation = Operation.parse(operationString);

                if (operation == null)
                {
                    throw new InvalidFormatException("Unknown operation type: " + operationString);
                }

                outerInstance.operationTable[word.getToken(0)] = operation;
            }
        }

        internal virtual DetokenizationDictionary.Operation getOperation(string token)
        {
            return operationTable[token];
        }

        // serialize method
        public virtual void serialize(OutputStream @out)
        {
            IEnumerator<Entry> entries = new IteratorAnonymousInnerClassHelper(this);

            DictionarySerializer.serialize(@out, entries, false);
        }

        private class IteratorAnonymousInnerClassHelper : IEnumerator<Entry>
        {
            private readonly DetokenizationDictionary outerInstance;

            public IteratorAnonymousInnerClassHelper(DetokenizationDictionary outerInstance)
            {
                this.outerInstance = outerInstance;
                iterator = outerInstance.operationTable.Keys.GetEnumerator();
            }


            internal IEnumerator<string> iterator;

            public virtual bool hasNext()
            {
                return iterator.MoveNext();
            }

            public virtual Entry next()
            {
                iterator.MoveNext();
                string token = iterator.Current;

                Attributes attributes = new Attributes();
                attributes.setValue("operation", outerInstance.getOperation(token).ToString());

                return new Entry(new StringList(token), attributes);
            }

            public virtual void remove()
            {
                throw new System.NotSupportedException();
            }

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }

            public bool MoveNext()
            {
                throw new System.NotImplementedException();
            }

            public void Reset()
            {
                throw new System.NotImplementedException();
            }

            public Entry Current { get; private set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}