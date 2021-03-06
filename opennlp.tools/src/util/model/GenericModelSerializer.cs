﻿using System.Collections.Generic;
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


namespace opennlp.tools.util.model
{
    using AbstractModel = opennlp.model.AbstractModel;
    using BinaryFileDataReader = opennlp.model.BinaryFileDataReader;
    using GenericModelReader = opennlp.model.GenericModelReader;

    public class GenericModelSerializer : ArtifactSerializer<AbstractModel>
    {
        public virtual AbstractModel create(InputStream @in)
        {
            return (new GenericModelReader(new BinaryFileDataReader(@in))).Model;
        }

        public virtual void serialize(AbstractModel artifact, OutputStream @out)
        {
            ModelUtil.writeModel(artifact, @out);
        }

        public static void register(IDictionary<string, ArtifactSerializer<AbstractModel>> factories)
        {
            factories.Add("model", new GenericModelSerializer());
        }
    }
}