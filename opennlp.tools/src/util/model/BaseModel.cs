using System;
using System.Collections.Generic;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.Utils;
using opennlp.model;
using opennlp.tools.dictionary;

namespace opennlp.tools.util.model
{
    public class BaseModel<T>
    {
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
            createBaseArtifactSerializers(artifactSerializers);

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.zip.ZipInputStream zip = new java.util.zip.ZipInputStream(in);
            ZipInputStream zip = new ZipInputStream(@in);

            // will read it in two steps, first using the known factories, latter the
            // unknown.
            leftoverArtifacts = new Dictionary<string, sbyte[]>();

            ZipEntry entry;
            while ((entry = zip.NextEntry) != null)
            {

                string extension = getEntryExtension(entry.Name);
                var factory = artifactSerializers.GetSerializerFromName(extension);

                if (factory == null)
                {
                    /* TODO: find a better solution, that would consume less memory */
                    sbyte[] bytes = toByteArray(zip);
                    leftoverArtifacts[entry.Name] = bytes;
                }
                else
                {
                    artifactMap[entry.Name] = factory.create(zip);
                }

                zip.closeEntry();
            }

            initializeFactory();

            loadArtifactSerializers();
            finishLoadingArtifacts();
            checkArtifactMap();
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

        private void createBaseArtifactSerializers(ArtifactSerializers serializers)
        {
            //JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
            serializers.putAll(createArtifactSerializers());
        }

        private ArtifactSerializers createArtifactSerializers()
        {
            var serializers = new ArtifactSerializers();
            
            serializers.Add<ArtifactSerializer<AbstractModel>>("model", new GenericModelSerializer());
            serializers.Add<ArtifactSerializer<Dictionary>>("dictionary", new DictionarySerializer());
            serializers.Add<ArtifactSerializer<Properties>>("properties", new PropertiesSerializer());

            return serializers;
        }

        private void initializeFactory()
        {
            
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

        private sbyte[] toByteArray(InputStream stream)
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
    }
}
