using opennlp.model;

namespace opennlp.nonjava.helperclasses
{
    public static class Arrays
    {
        internal static T[] CopyOf<T>(T[] original, int newLength)
        {
            T[] dest = new T[newLength];
            System.Array.Copy(original, dest, newLength);
            return dest;
        }

        internal static T[] CopyOfRange<T>(T[] original, int fromIndex, int toIndex)
        {
            int length = toIndex - fromIndex;
            T[] dest = new T[length];
            System.Array.Copy(original, fromIndex, dest, 0, length);
            return dest;
        }

        internal static void Fill<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        internal static void Fill<T>(T[] array, int fromIndex, int toIndex, T value)
        {
            for (int i = fromIndex; i < toIndex; i++)
            {
                array[i] = value;
            }
        }

        public static void sort(int[] pids)
        {
            throw new System.NotImplementedException();
        }

        public static string asList(string[] context)
        {
            throw new System.NotImplementedException();
        }

        public static decimal binarySearch(int[] outcomes, int outcome)
        {
            throw new System.NotImplementedException();
        }

        public static void fill(double[] rho, double naN)
        {
            throw new System.NotImplementedException();
        }

        public static void sort(string[] sortedPredLabels)
        {
            throw new System.NotImplementedException();
        }

        public static void sort(char[] sortedPredLabels)
        {
            throw new System.NotImplementedException();
        }
    }
}