using System;

namespace j4n.IO.Writer
{
    public class BufferedWriter : Writer
    {
        public BufferedWriter(OutputStreamWriter outputStreamWriter)
            :base(outputStreamWriter.InnerStream)
        {
        }

        public void newLine()
        {
            write(Environment.NewLine);
        }
    }
}