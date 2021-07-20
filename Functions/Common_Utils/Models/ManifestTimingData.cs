// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Common_Utils
{
    public class ManifestTimingData
    {
        public TimeSpan AssetDuration { get; set; }
        public ulong TimestampOffset { get; set; }
        public long? TimeScale { get; set; }
        public bool IsLive { get; set; }
        public bool Error { get; set; }
        public List<ulong> TimestampList { get; set; }
        public ulong TimestampEndLastChunk { get; set; }
        public bool DiscontinuityDetected { get; set; }
    }

}