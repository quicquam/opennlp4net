using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using opennlp.model;
using opennlp.tools.util.model;

namespace opennlp.tools.util
{
    public abstract class BaseToolFactory
    {
        protected internal ArtifactProvider artifactProvider;

        /// Initializes the ToolFactory with an artifact provider.
        /// </summary>
        protected internal virtual void init(ArtifactProvider artifactProvider)
        {
            this.artifactProvider = artifactProvider;
        }

        public virtual void validateArtifactMap()
        {
            throw new NotImplementedException();
        }

        public virtual IDictionary<string, object> createArtifactMap()
        {
            throw new NotImplementedException();
        }

        public virtual IDictionary<string, string> createManifestEntries()
        {
            throw new NotImplementedException();
        }

        public static BaseToolFactory create(Type factoryClass)
        {
            BaseToolFactory theFactory = null;
            if (factoryClass != null)
            {
                try
                {
                    theFactory = (BaseToolFactory) Activator.CreateInstance(factoryClass);
                    // Do the init in caller to avoid non-static in static error
                    //theFactory.init(artifactProvider);
                }
                catch (Exception e)
                {
                    string msg = "Could not instantiate the " + factoryClass.FullName +
                                 ". The initialization throw an exception.";
                    Console.Error.WriteLine(msg);
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                    throw new InvalidFormatException(msg, e);
                }
            }
            return theFactory;
        }

        public static BaseToolFactory create(string factoryClass)
        {
            BaseToolFactory theFactory = null;

            try
            {
                theFactory = (BaseToolFactory) Activator.CreateInstance(GetTypeFromClassname(factoryClass));

                if (theFactory != null)
                {
                    // Do the init in caller to avoid non-static in static error
                    //theFactory.init(artifactProvider);
                }
            }
            catch (Exception e)
            {
                string msg = "Could not instantiate the " + factoryClass + ". The initialization throw an exception.";
                Console.Error.WriteLine(msg);
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                throw new InvalidFormatException(msg, e);
            }
            return theFactory;
        }

        private static Type GetTypeFromClassname(string factoryClass)
        {
            return Type.GetType(factoryClass);
        }

        protected IDictionary<string, ArtifactSerializer<object>> createArtifactSerializersMap()
        {
            throw new NotImplementedException();
        }
    }
}