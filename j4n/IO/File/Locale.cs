using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.IO.File
{
    public class Locale
    {
        public Locale(string language)
        {
            throw new NotImplementedException();
        }

        public static CultureInfo ENGLISH { get; set; }
        public static Locale Default { get; set; }
        public static string[] ISOLanguages { get; set; }

        public CultureInfo GetCultureInfo()
        {
            throw new NotImplementedException();
        }
    }
}