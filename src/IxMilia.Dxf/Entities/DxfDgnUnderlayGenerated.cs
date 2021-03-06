// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// The contents of this file are automatically generated by a tool, and should not be directly modified.

using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dxf.Entities
{

    /// <summary>
    /// DxfDgnUnderlay class
    /// </summary>
    public partial class DxfDgnUnderlay : DxfUnderlay
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.DgnUnderlay; } }


        public DxfDgnUnderlay()
            : base()
        {
        }

        internal DxfDgnUnderlay(DxfUnderlay other)
            : base(other)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void AddValuePairs(List<DxfCodePair> pairs, DxfAcadVersion version, bool outputHandles)
        {
            base.AddValuePairs(pairs, version, outputHandles);
        }
    }

}
