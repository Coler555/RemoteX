﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteX.Service
{
    public interface IVibrator
    {
        bool HasVibrator { get; }
        void Vibrate();
    }
}
