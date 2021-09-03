﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TAO3.Converters;

namespace TAO3.Internal.Commands.Output
{
    internal class DefaultOutputConfigurableConverter<TSettings, TCommandParameters>
        : IOutputConfigurableConverter<TSettings, TCommandParameters>
    {
        public void Configure(Command command)
        {
            
        }

        public TSettings GetDefaultSettings()
        {
            return default!;
        }
    }
}
