﻿using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using j4n.Exceptions;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.Utils;
using opennlp.model;
using opennlp.tools.dictionary;
using opennlp.tools.parser;


namespace opennlp.tools.util.model
{
    public class BaseModel : ArtifactProvider
    {
        private static int MODEL_BUFFER_SIZE_LIMIT = Int32.MaxValue;

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

        private static String SERIALIZER_CLASS_NAME_PREFIX = "serializer-class-";

        private bool subclassSerializersInitiated = false;
        private bool finishedLoadingArtifacts = false;

        public string Language { get; set; }
        public bool LoadedFromSerialized { get; private set; }

        protected internal readonly IDictionary<string, object> artifactMap = new Dictionary<string, object>();
        protected ArtifactSerializers artifactSerializers;

        protected internal BaseToolFactory toolFactory;
        private bool isLoadedFromSerialized;
        private string _componentName;
        private Dictionary<string, byte[]> leftoverArtifacts;

        private BaseModel(string componentName, bool isLoadedFromSerialized)
        {
            this.isLoadedFromSerialized = isLoadedFromSerialized;

            if (componentName == null)
            {
                throw new System.ArgumentException("_componentName must not be null!");
            }

            _componentName = componentName;
        }

        protected BaseModel(string componentName, string languageCode, IEnumerable<KeyValuePair<string, string>> manifestInfoEntries,
            BaseToolFactory factory)
        {
            _componentName = componentName;
            if (languageCode == null)
                throw new IllegalArgumentException("languageCode must not be null!");

            createBaseArtifactSerializers();

            Properties manifest = new Properties();
            manifest.setProperty(MANIFEST_VERSION_PROPERTY, "1.0");
            manifest.setProperty(LANGUAGE_PROPERTY, languageCode);
            manifest.setProperty(VERSION_PROPERTY, Version.currentVersion().ToString());
            manifest.setProperty(TIMESTAMP_PROPERTY, DateTime.Now.Ticks.ToString());
            manifest.setProperty(COMPONENT_NAME_PROPERTY, componentName);

            if (manifestInfoEntries != null)
            {
                foreach (KeyValuePair<string, string> manifestInfoEntry in manifestInfoEntries)
                {
                    manifest.Add(manifestInfoEntry.Key, manifestInfoEntry.Value);
                }
            }

            artifactMap.Add(MANIFEST_ENTRY, manifest);
            finishedLoadingArtifacts = true;

            if (factory != null)
            {
                setManifestProperty(FACTORY_NAME, factory.getClass().FullName);
                foreach (KeyValuePair<string, object> keyValuePair in factory.createArtifactMap())
                {
                    artifactMap.Add(keyValuePair.Key, keyValuePair.Value);
                }

                // new manifest entries
                IDictionary<String, String> entries = factory.createManifestEntries();
                foreach (KeyValuePair<string, string> keyValuePair in entries)
                {
                    setManifestProperty(keyValuePair.Key, keyValuePair.Value);
                }
            }
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

        protected BaseModel(string namefinderme, string languageCode, IDictionary<string, string> manifestInfoEntries)
        {
            throw new NotImplementedException();
        }

        private void loadModel(InputStream @in)
        {
            createBaseArtifactSerializers();

            using (var zip = new ZipInputStream(@in.InnerStream))
            {
                // will read it in two steps, first using the known factories, latter the
                // unknown.
                leftoverArtifacts = new Dictionary<string, byte[]>();

                ZipEntry entry;
                while ((entry = zip.GetNextEntry()) != null)
                {
                    string extension = getEntryExtension(entry.FileName);
                    var factory = artifactSerializers.GetValueObject(extension);

                    if (factory == null)
                    {
                        /* TODO: find a better solution, that would consume less memory */
                        var data = new byte[entry.UncompressedSize];
                        zip.Read(data, 0, data.Length);
                        leftoverArtifacts[entry.FileName] = data;
                    }
                    else
                    {
                        var data = new byte[entry.UncompressedSize];
                        zip.Read(data, 0, (int)entry.UncompressedSize);
                        var stream = new MemoryStream(data);
                        artifactMap[entry.FileName] = CreateConcreteType(factory, new InputStream(stream));

                        // entry.IsDirectory doesn't work, but a CompressionRatio less than zero
                        // indicates a directory (no data, just headers), move the zip.Position back by 
                        // the size of the directory record
                        if (entry.CompressionRatio < 0)
                            zip.Seek(-16, SeekOrigin.Current);
                    }
                }

                initializeFactory();

                loadArtifactSerializers();
                finishLoadingArtifacts(@in);
            }

            checkArtifactMap();
        }
       
        private void initializeFactory()
        {
            string factoryName = getManifestProperty(FACTORY_NAME);
            if (factoryName == null)
            {
                // load the default factory
                Type factoryClass = DefaultFactory;
                if (factoryClass != null)
                {
                    this.toolFactory = BaseToolFactory.create(factoryClass);
                }
            }
            else
            {
                try
                {
                    this.toolFactory = BaseToolFactory.create(factoryName);
                }
                catch (InvalidFormatException e)
                {
                    throw new System.ArgumentException(e.Message);
                }
            }
            if(toolFactory != null)
                this.toolFactory.init(this);
        }

        private object CreateConcreteType(object factory, InputStream inputStream)
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
            if (factory is ParserModel.POSModelSerializer)
            {
                var serializer = factory as ParserModel.POSModelSerializer;
                return serializer.create(inputStream);
            }
            if (factory is ParserModel.HeadRulesSerializer)
            {
                var serializer = factory as ParserModel.HeadRulesSerializer;
                return serializer.create(inputStream);
            }
            if (factory is ParserModel.ChunkerModelSerializer)
            {
                var serializer = factory as ParserModel.ChunkerModelSerializer;
                return serializer.create(inputStream);
            }

            return null;
        }

        private void PersistConcreteType(object factory, object artifact, ZipOutputStream zipOutputStream)
        {
            if (factory is PropertiesSerializer)
            {
                var serializer = factory as PropertiesSerializer;
                var properties = artifact as Properties;
                if(properties != null)
                    serializer.serialize(properties, new OutputStream(zipOutputStream));
            }
            if (factory is GenericModelSerializer)
            {
                var serializer = factory as GenericModelSerializer;
                var model = artifact as AbstractModel;
                if (model != null)
                    serializer.serialize(model, new OutputStream(zipOutputStream));
            }
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

        public static IDictionary<string, ArtifactSerializer<Object>> staticCreateBaseArtifactSerializers()
        {
            throw new NotImplementedException();
        }

        protected void createBaseArtifactSerializers()
        {
            createArtifactSerializers();
        }

        public virtual void createArtifactSerializers()
        {
            if (artifactSerializers == null)
                artifactSerializers = new ArtifactSerializers();

            if (!artifactSerializers.Contains("model"))
                artifactSerializers.Add<ArtifactSerializer<AbstractModel>>("model", new GenericModelSerializer());

            if (!artifactSerializers.Contains("dictionary"))
                artifactSerializers.Add<ArtifactSerializer<Dictionary>>("dictionary", new DictionarySerializer());

            if (!artifactSerializers.Contains("properties"))
                artifactSerializers.Add<ArtifactSerializer<Properties>>("properties", new PropertiesSerializer());
        }

        private void loadArtifactSerializers()
        {
            if (!subclassSerializersInitiated)
                createArtifactSerializers();
            subclassSerializersInitiated = true;
        }

        private void finishLoadingArtifacts(InputStream @in)
        {
            var zip = new ZipInputStream(@in.GetStream());

            ZipEntry entry;
            while ((entry = zip.GetNextEntry()) != null)
            {
                // Note: The manifest.properties file will be read here again,
                // there should be no need to prevent that.

                String entryName = entry.FileName;
                String extension = getEntryExtension(entryName);

                var factory = artifactSerializers.GetValueObject(extension);

                String artifactSerializerClazzName =
                    getManifestProperty(SERIALIZER_CLASS_NAME_PREFIX + entryName);

                if (artifactSerializerClazzName != null)
                {
                    factory = artifactSerializers.GetValueObject(artifactSerializerClazzName);
                }

                if (factory != null)
                {
                    artifactMap.Add(entryName, CreateConcreteType(factory, @in));
                }
                else
                {
                    throw new InvalidFormatException("Unknown artifact format: " + extension);
                }

                zip.Close();
            }

            finishedLoadingArtifacts = true;
        }

        protected internal virtual void validateArtifactMap()
        {
            var properties = artifactMap[MANIFEST_ENTRY] as Properties;
            if (properties == null)
            {
                throw new InvalidFormatException("Missing the " + MANIFEST_ENTRY + "!");
            }

            // First check version, everything else might change in the future
            String versionString = getManifestProperty(VERSION_PROPERTY);

            if (versionString != null)
            {
                Version version;

                try
                {
                    version = Version.parse(versionString);
                }
                catch (NumberFormatException e)
                {
                    throw new InvalidFormatException("Unable to parse model version '" + versionString + "'!", e);
                }

                // Version check is only performed if current version is not the dev/debug version
                if (!Version.currentVersion().Equals((Version.DEV_VERSION)))
                {
                    // Major and minor version must match, revision might be
                    if (Version.currentVersion().getMajor() != version.getMajor() ||
                        Version.currentVersion().getMinor() != version.getMinor())
                    {
                        //this check allows for the use of models one minor release behind current minor release
                        if (Version.currentVersion().getMajor() == version.getMajor() &&
                            (Version.currentVersion().getMinor() - 1) != version.getMinor())
                        {
                            throw new InvalidFormatException("Model version " + version + " is not supported by this ("
                                                             + Version.currentVersion() + ") version of OpenNLP!");
                        }
                    }

                    // Reject loading a snapshot model with a non-snapshot version
                    if (!Version.currentVersion().isSnapshot() && version.isSnapshot())
                    {
                        throw new InvalidFormatException("Model version " + version +
                                                         " is a snapshot - snapshot models are not " +
                                                         "supported by this non-snapshot version (" +
                                                         Version.currentVersion() + ") of OpenNLP!");
                    }
                }
            }
            else
            {
                throw new InvalidFormatException("Missing " + VERSION_PROPERTY + " property in " +
                                                 MANIFEST_ENTRY + "!");
            }

            if (getManifestProperty(COMPONENT_NAME_PROPERTY) == null)
                throw new InvalidFormatException("Missing " + COMPONENT_NAME_PROPERTY + " property in " +
                                                 MANIFEST_ENTRY + "!");

            if (!getManifestProperty(COMPONENT_NAME_PROPERTY).Equals(_componentName))
                throw new InvalidFormatException("The " + _componentName + " cannot load a model for the " +
                                                 getManifestProperty(COMPONENT_NAME_PROPERTY) + "!");

            if (getManifestProperty(LANGUAGE_PROPERTY) == null)
                throw new InvalidFormatException("Missing " + LANGUAGE_PROPERTY + " property in " +
                                                 MANIFEST_ENTRY + "!");

            // Validate the factory. We try to load it using the ExtensionLoader. It
            // will return the factory, null or raise an exception
            String factoryName = getManifestProperty(FACTORY_NAME);
            if (factoryName != null)
            {
                try
                {
                    if (BaseToolFactory.create(factoryName) == null)
                    {
                        throw new InvalidFormatException(
                            "Could not load an user extension specified by the model: "
                            + factoryName);
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidFormatException(
                        "Could not load an user extension specified by the model: "
                        + factoryName, e);
                }
            }

            // validate artifacts declared by the factory
            if (toolFactory != null)
            {
                toolFactory.validateArtifactMap();
            }
        }

        protected internal virtual void checkArtifactMap()
        {
            if (!finishedLoadingArtifacts)
                throw new IllegalStateException(
                    "The method BaseModel.finishLoadingArtifacts(..) was not called by BaseModel sub-class.");
            try
            {
                validateArtifactMap();
            }
            catch (InvalidFormatException e)
            {
                throw new IllegalArgumentException(e.Message);
            }
        }

        public void serialize(FileOutputStream fileOutputStream)
        {
            using (var zip = new ZipOutputStream(fileOutputStream.InnerStream))
            {
                foreach (KeyValuePair<string, object> kvp in artifactMap)
                {
                    string extension = getEntryExtension(kvp.Key);
                    object artifact = artifactSerializers.GetValueObject(extension);
                    zip.PutNextEntry(kvp.Key);
                    PersistConcreteType(artifact, kvp.Value, zip);
                }
            }
        }

        protected internal virtual Type DefaultFactory
        {
            get { return null; }
        }

        public T getArtifact<T>(string key)
        {
            if (artifactMap.ContainsKey(key))
            {
                var artifact = artifactMap[key];
                return (T) artifact;
            }
            return default(T);
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
            var manifest = (Properties) artifactMap[MANIFEST_ENTRY];
            return manifest.getProperty(key);
        }

        public void setManifestProperty(string key, string value)
        {
            var manifest = (Properties) artifactMap[MANIFEST_ENTRY];
            manifest.setProperty(key, value);
        }
    }
}