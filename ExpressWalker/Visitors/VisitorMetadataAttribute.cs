﻿using System;

namespace ExpressWalker.Visitors
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VisitorMetadataAttribute : Attribute
    {
        public object Metadata { get; private set; }

        public VisitorMetadataAttribute(object metadata)
        {
            Metadata = metadata;
        }
    }
}
