namespace j4n.IO.InputStream
{
    public class BufferedInputStream : InputStream
    {
        public BufferedInputStream(string path)
            : base(path)
        {
        }

        public BufferedInputStream(InputStream fileInputStream)
            : base(fileInputStream.Path)
        {
            throw new System.NotImplementedException();
        }

        public BufferedInputStream(InputStream fileInputStream, int ioBufferSize)
            : base(fileInputStream)
        {
            throw new System.NotImplementedException();
        }
    }
}