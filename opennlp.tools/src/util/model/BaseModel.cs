using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using opennlp.model;
using opennlp.tools.dictionary;


namespace opennlp.tools.util.model
{
    public class BaseModel<T>
    {
        protected internal const string MANIFEST_ENTRY = "manifest.properties";
        protected internal const string FACTORY_NAME = "factory";

        private const string MANIFEST_VERSION_PROPERTY = "Manifest-Version";
        private const string COMPONENT_NAME_PROPERTY = "Component-Name";
        private const string VERSION_PROPERTY = "OpenNLP-Version";
        private const string TIMESTAMP_PROPERTY = "Timestamp";
        private const string LANGUAGE_PROPERTY = "Language";

        public const string TRAINING_CUTOFF_PROPERTY = "Training-Cutoff";
        public const string TRAINING_ITERATIONS_PROPERTY = "Training-Iterations";
        public const string TRAINING_EVENTHASH_PROPERTY = "Training-Eventhash";


        public string Language { get; set; }

        protected internal readonly IDictionary<string, object> artifactMap = new Dictionary<string, object>();
        private ArtifactSerializers artifactSerializers;

        protected internal BaseToolFactory toolFactory;
        private bool isLoadedFromSerialized;
        private string componentName;
        private Dictionary<string, sbyte[]> leftoverArtifacts;

        private BaseModel(string componentName, bool isLoadedFromSerialized)
        {
            this.isLoadedFromSerialized = isLoadedFromSerialized;

            if (componentName == null)
            {
                throw new System.ArgumentException("componentName must not be null!");
            }

            this.componentName = componentName;
        }

        protected BaseModel(string componentName, string languageCode, IDictionary<string, string> manifestInfoEntries, BaseToolFactory factory)
        {
            throw new NotImplementedException();
        }

        protected BaseModel(string componentName, InputStream @in)
            : this(componentName, true)
        {

            if (@in == null)
            {
                throw new System.ArgumentException("in must not be null!");
            }

            loadModel(@in);
        }

        protected BaseModel(string componentName, Jfile languageCode)
        {
            throw new NotImplementedException();
        }

        protected BaseModel(string componentName, Uri languageCode)
        {
            throw new NotImplementedException();
        }

        private void loadModel(InputStream @in)
        {
            createBaseArtifactSerializers();

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.zip.ZipInputStream zip = new java.util.zip.ZipInputStream(in);
            using (var zip = new ZipInputStream(@in.Stream))
            {
                // will read it in two steps, first using the known factories, latter the
                // unknown.
                leftoverArtifacts = new Dictionary<string, sbyte[]>();

                ZipEntry entry;
                while ((entry = zip.GetNextEntry()) != null)
                {

                    string extension = getEntryExtension(entry.FileName);
                    var factory = artifactSerializers.GetValueObject(extension);

                    if (factory == null)
                    {
                        /* TODO: find a better solution, that would consume less memory */
                        sbyte[] bytes = toByteArray(zip);
                        leftoverArtifacts[entry.FileName] = bytes;
                    }
                    else
                    {
                        var data = new byte[entry.UncompressedSize];
                        zip.Read(data, 0, data.Length);
                        var stream = new MemoryStream(data);
                        artifactMap[entry.FileName] = GetConcreteType(factory, new InputStream(stream));
                    }
                }
            }

            initializeFactory();

            loadArtifactSerializers();
            finishLoadingArtifacts();
            checkArtifactMap();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void initializeFactory() throws opennlp.tools.util.InvalidFormatException
        private void initializeFactory()
        {
            string factoryName = getManifestProperty(FACTORY_NAME);
            if (factoryName == null)
            {
                // load the default factory
                Type factoryClass = DefaultFactory;
                if (factoryClass != null)
                {
                   // this.toolFactory = BaseToolFactory.create(factoryClass, this);
                }
            }
            else
            {
                try
                {
                   // this.toolFactory = BaseToolFactory.create(factoryName, this);
                }
                catch (InvalidFormatException e)
                {
                    throw new System.ArgumentException(e.Message);
                }
            }
        }


        private object GetConcreteType(object factory, InputStream inputStream)
        {
            if (factory is PropertiesSerializer)
            {
                var serializer = factory as PropertiesSerializer;
                return serializer.create(inputStream);
            }
            if (factory is GenericModelSerializer)
            {
                var serializer = factory as GenericModelSerializer;
                return serializer.create(inputStream);
            }
            if (factory is DictionarySerializer)
            {
                var serializer = factory as DictionarySerializer;
                return serializer.create(inputStream);
            }
            return null;
        }

        private string getEntryExtension(string entry)
        {
            int extensionIndex = entry.LastIndexOf('.') + 1;

            if (extensionIndex == -1 || extensionIndex >= entry.Length)
            {
                throw new InvalidFormatException("Entry name must have type extension: " + entry);
            }

            return entry.Substring(extensionIndex);
        }

        private void createBaseArtifactSerializers()
        {
            //JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
            artifactSerializers = createArtifactSerializers();
        }

        private ArtifactSerializers createArtifactSerializers()
        {
            var serializers = new ArtifactSerializers();
            
            serializers.Add<ArtifactSerializer<AbstractModel>>("model", new GenericModelSerializer());
            serializers.Add<ArtifactSerializer<Dictionary>>("dictionary", new DictionarySerializer());
            serializers.Add<ArtifactSerializer<Properties>>("properties", new PropertiesSerializer());

            return serializers;
        }

        private void loadArtifactSerializers()
        {
        }

        private void finishLoadingArtifacts()
        {

        }        

        protected internal virtual void validateArtifactMap()
        {
            throw new NotImplementedException();
        }

        protected internal virtual void checkArtifactMap()
        {
            throw new NotImplementedException();
        }

        protected void serialize(FileOutputStream fileOutputStream)
        {
            throw new NotImplementedException();
        }

        private sbyte[] toByteArray(ZipInputStream stream)
        {
            throw new NotImplementedException();
        }

        protected virtual internal Type DefaultFactory
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the value to the given key from the manifest.properties
        /// entry.
        /// </summary>
        /// <param name="key">
        /// </param>
        /// <returns> the value </returns>
        public string getManifestProperty(string key)
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];

            return manifest.getProperty(key);
        }
    }
}
