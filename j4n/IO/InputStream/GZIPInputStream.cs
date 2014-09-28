namespace j4n.IO.InputStream
{
// ReSharper disable once InconsistentNaming
    public class GZIPInputStream : InputStream
    {
        public GZIPInputStream(BufferedInputStream bufferedInputStream)
            : base(bufferedInputStream.Path)
        {
            throw new System.NotImplementedException();
        }

        public GZIPInputStream(FileInputStream bufferedInputStream)
            : base(bufferedInputStream.Path)
        {
            throw new System.NotImplementedException();
        }
    }
}
