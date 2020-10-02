﻿using System;

namespace Bewit.Core
{
    public class BewitOptions
    {
        /// <summary>
        /// Secret used for the hash generation.
        /// Mandatory.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Duration of the Token.
        /// Optional. Default is 60 seconds
        /// </summary>
        public TimeSpan TokenDuration { get; set; }
    }
}
