using j4n.Interfaces;
using System;
using System.IO;
using System.Text;

namespace j4n.IO.Writer
{
    public class Writer : Closeable, Flushable
    {
        public Stream InnerStream;
        private readonly StreamWriter _streamWriter;

        public Writer(Stream stream, string encoding = null)
        {
            InnerStream = stream;
            var enc = encoding == "UTF-8" ? Encoding.UTF8 : Encoding.Default;
            _streamWriter = new StreamWriter(stream, enc);
        }

        public void close()
        {
            flush();
            InnerStream.Close();
        }

        public void flush()
        {
            InnerStream.Flush();
        }

        public void write(string str)
        {
            _streamWriter.Write(str);
        }

        public void write(char charValue)
        {
            _streamWriter.Write(charValue);
        }

        public void writeLine(string str = "")
        {
            _streamWriter.Write(str + Environment.NewLine);
        }
    }
}