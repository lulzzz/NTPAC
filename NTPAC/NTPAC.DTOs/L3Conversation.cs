﻿using System.Net;

namespace NTPAC.DTOs
{
    public class L3Conversation
    {
        public IPEndPoint DestinationEndPoint { get; set; }
        public IPEndPoint SourceEndPoint { get; set; }
    }
}
