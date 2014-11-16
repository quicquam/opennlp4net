using System;
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
using j4n.Exceptions;
using j4n.IO.InputStream;
using j4n.Utils;
using opennlp.tools.nonjava;
using opennlp.tools.nonjava.extensions;


namespace opennlp.tools.util
{
    /// <summary>
    /// The <seealso cref="Version"/> class represents the OpenNlp Tools library version.
    /// <para>
    /// The version has three parts:
    /// <ul>
    /// <li>Major: OpenNlp Tools libraries with a different major version are not interchangeable.</li>
    /// <li>Minor: OpenNlp Tools libraries with an identical major version, but different
    ///     minor version may be interchangeable. See release notes for further details.</li>
    /// <li>Revision: OpenNlp Tools libraries with same major and minor version, but a different
    ///     revision, are fully interchangeable.</li>
    /// </ul>
    /// </para>
    /// </summary>
    public class Version
    {
        private const string DEV_VERSION_STRING = "0.0.0-SNAPSHOT";

        public static readonly Version DEV_VERSION = Version.parse(DEV_VERSION_STRING);

        private const string SNAPSHOT_MARKER = "-SNAPSHOT";

        private readonly int major;

        private readonly int minor;

        private readonly int revision;

        private readonly bool snapshot;

        /// <summary>
        /// Initializes the current instance with the provided
        /// versions.
        /// </summary>
        /// <param name="major"> </param>
        /// <param name="minor"> </param>
        /// <param name="revision"> </param>
        /// <param name="snapshot"> </param>
        public Version(int major, int minor, int revision, bool snapshot)
        {
            this.major = major;
            this.minor = minor;
            this.revision = revision;
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Initializes the current instance with the provided
        /// versions. The version will not be a snapshot version.
        /// </summary>
        /// <param name="major"> </param>
        /// <param name="minor"> </param>
        /// <param name="revision"> </param>
        public Version(int major, int minor, int revision) : this(major, minor, revision, false)
        {
        }


        /**
 * Retrieves the major version.
 *
 * @return major version
 */

        public int getMajor()
        {
            return major;
        }

        /**
       * Retrieves the minor version.
       *
       * @return minor version
       */

        public int getMinor()
        {
            return minor;
        }

        /**
       * Retrieves the revision version.
       *
       * @return revision version
       */

        public int getRevision()
        {
            return revision;
        }

        public bool isSnapshot()
        {
            return snapshot;
        }

        /// <summary>
        /// Retrieves the major version.
        /// </summary>
        /// <returns> major version </returns>
        public virtual int Major
        {
            get { return major; }
        }

        /// <summary>
        /// Retrieves the minor version.
        /// </summary>
        /// <returns> minor version </returns>
        public virtual int Minor
        {
            get { return minor; }
        }

        /// <summary>
        /// Retrieves the revision version.
        /// </summary>
        /// <returns> revision version </returns>
        public virtual int Revision
        {
            get { return revision; }
        }

        public virtual bool Snapshot
        {
            get { return snapshot; }
        }

        /// <summary>
        /// Retrieves the version string.
        /// 
        /// The <seealso cref="#parse(String)"/> method can create an instance
        /// of <seealso cref="Version"/> with the returned version value string.
        /// </summary>
        /// <returns> the version value string </returns>
        public override string ToString()
        {
            return Convert.ToString(Major) + "." + Convert.ToString(Minor) + "." + Convert.ToString(Revision) +
                   (Snapshot ? SNAPSHOT_MARKER : "");
        }

        public override bool Equals(object o)
        {
            if (o == this)
            {
                return true;
            }
            else if (o is Version)
            {
                Version version = (Version) o;

                return Major == version.Major && Minor == version.Minor && Revision == version.Revision &&
                       Snapshot == version.Snapshot;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Return a new <seealso cref="Version"/> initialized to the value
        /// represented by the specified <seealso cref="String"/>
        /// </summary>
        /// <param name="version"> the string to be parsed
        /// </param>
        /// <returns> the version represented by the string value
        /// </returns>
        /// <exception cref="NumberFormatException"> if the string does
        /// not contain a valid version </exception>
        public static Version parse(string version)
        {
            int indexFirstDot = version.IndexOf('.');

            int indexSecondDot = version.IndexOf('.', indexFirstDot + 1);

            if (indexFirstDot == -1 || indexSecondDot == -1)
            {
                throw new NumberFormatException("Invalid version format '" + version + "', expected two dots!");
            }

            int indexFirstDash = version.IndexOf('-');

            int versionEnd;
            if (indexFirstDash == -1)
            {
                versionEnd = version.Length;
            }
            else
            {
                versionEnd = indexFirstDash;
            }

            bool snapshot = version.EndsWith(SNAPSHOT_MARKER, StringComparison.Ordinal);

            return new Version(Convert.ToInt32(version.Substring(0, indexFirstDot)),
                Convert.ToInt32(version.SubstringSpecial(indexFirstDot + 1, indexSecondDot)),
                Convert.ToInt32(StringHelperClass.SubstringSpecial(version, indexSecondDot + 1, versionEnd)), snapshot);
        }

        /// <summary>
        /// Retrieves the current version of the OpenNlp Tools library.
        /// </summary>
        /// <returns> the current version </returns>
        public static Version currentVersion()
        {
            Properties manifest = new Properties();

            // Try to read the version from the version file if it is available,
            // otherwise set the version to the development version

            InputStream versionIn = null; //typeof(Version).getResourceAsStream("opennlp.version");

            if (versionIn != null)
            {
                try
                {
                    manifest.load(versionIn);
                }
                catch (IOException)
                {
                    // ignore error
                }
                finally
                {
                    try
                    {
                        versionIn.close();
                    }
                    catch (IOException)
                    {
                        // ignore error
                    }
                }
            }

            string versionString = manifest.getProperty("OpenNLP-Version", DEV_VERSION_STRING);

            if (versionString.Equals("${pom.version}"))
            {
                versionString = DEV_VERSION_STRING;
            }

            return Version.parse(versionString);
        }
    }
}