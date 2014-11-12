//----------------------------------------------------------------------------------------
//	Copyright © 2007 - 2014 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class provides the logic to simulate Java rectangular arrays, which are jagged
//	arrays with inner arrays of the same length. A size of -1 indicates unknown length.
//----------------------------------------------------------------------------------------

namespace opennlp.nonjava.helperclasses
{
    internal static class RectangularArrays
    {
        internal static float[][] ReturnRectangularFloatArray(int Size1, int Size2)
        {
            float[][] Array;
            if (Size1 > -1)
            {
                Array = new float[Size1][];
                if (Size2 > -1)
                {
                    for (int Array1 = 0; Array1 < Size1; Array1++)
                    {
                        Array[Array1] = new float[Size2];
                    }
                }
            }
            else
                Array = null;

            return Array;
        }

        internal static double[][] ReturnRectangularDoubleArray(int Size1, int Size2)
        {
            double[][] Array;
            if (Size1 > -1)
            {
                Array = new double[Size1][];
                if (Size2 > -1)
                {
                    for (int Array1 = 0; Array1 < Size1; Array1++)
                    {
                        Array[Array1] = new double[Size2];
                    }
                }
            }
            else
                Array = null;

            return Array;
        }

        internal static int[][][] ReturnRectangularIntArray(int Size1, int Size2, int Size3)
        {
            int[][][] Array;
            if (Size1 > -1)
            {
                Array = new int[Size1][][];
                if (Size2 > -1)
                {
                    for (int Array1 = 0; Array1 < Size1; Array1++)
                    {
                        Array[Array1] = new int[Size2][];
                        if (Size3 > -1)
                        {
                            for (int Array2 = 0; Array2 < Size2; Array2++)
                            {
                                Array[Array1][Array2] = new int[Size3];
                            }
                        }
                    }
                }
            }
            else
                Array = null;

            return Array;
        }
    }
}