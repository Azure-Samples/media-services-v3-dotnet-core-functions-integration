// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Common_Utils
{
    public class ManifestSegmentData
    {
        public ulong timestamp;
        public bool calculated; // it means the timestamp has been calculated from previous
        public bool timestamp_mismatch; // if there is a mismatch
    }
}