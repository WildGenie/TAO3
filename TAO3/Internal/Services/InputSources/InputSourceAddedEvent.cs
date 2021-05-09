﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAO3.InputSources;

namespace TAO3.Internal.Services
{
    internal class InputSourceAddedEvent
    {
        public IInputSource InputSource { get; }

        public InputSourceAddedEvent(IInputSource inputSource)
        {
            InputSource = inputSource;
        }
    }
}
