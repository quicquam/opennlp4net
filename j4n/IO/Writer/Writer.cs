using System;
using System.IO;
using System.Text;
using j4n.Interfaces;
using j4n.IO.OutputStream;

namespace j4n.IO.Writer
{
    public class Writer : Closeable, Flushable
    {
        public Stream InnerStream;
        private StreamWriter _streamWriter;

        public Writer(Stream stream, string encoding = null)
        {
            InnerStream = stream;
            var enc = encoding == "UTF-8" ? Encoding.UTF8 : Encoding.Default;
            _streamWriter = new StreamWriter(stream, enc);
        }

        public void close()
        {
            InnerStream.Close();
        }

        public void flush()
        {
            InnerStream.Flush();
        }

        public void write(string str)
        {
            _streamWriter.Write(str);
            //var bytes = GetBytes(str);
            //InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void write(char charValue)
        {
            _streamWriter.Write(charValue);
            //var bytes = BitConverter.GetBytes(charValue);
            //InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        public void writeLine(string str = "")
        {
            _streamWriter.Write(str + Environment.NewLine);
            //var bytes = GetBytes(str + Environment.NewLine);
            //InnerStream.Write(bytes, 0, bytes.GetLength(0));
        }

        private byte[] GetBytes(string str)
        {
            throw new NotImplementedException();
            //var bytes = new byte[str.Length*sizeof (char)];
            //Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            //return bytes;
        }
    }
}