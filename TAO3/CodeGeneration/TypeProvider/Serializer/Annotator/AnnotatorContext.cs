﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAO3.TypeProvider
{
    public class AnnotatorContext
    {
        public StringBuilder StringBuilder { get; }
        public HashSet<string> Namespaces { get; }
        public string Format { get; }

        public AnnotatorContext(StringBuilder stringBuilder, HashSet<string> namespaces, string format)
        {
            StringBuilder = stringBuilder;
            Namespaces = namespaces;
            Format = format;
        }

        public void Using(string @namespace)
        {
            Namespaces.Add(@namespace);
        }
    }
}
