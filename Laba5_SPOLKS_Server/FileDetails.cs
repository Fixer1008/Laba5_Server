﻿using System;
using System.Text;

namespace Laba5_SPOLKS_Server
{
    [Serializable]
    public class FileDetails
    {
        public string FileName { get; set; }
        public long FileLength { get; set; }
    }
}
