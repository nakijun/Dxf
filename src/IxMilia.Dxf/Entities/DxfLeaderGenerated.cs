// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// The contents of this file are automatically generated by a tool, and should not be directly modified.

using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dxf.Entities
{

    /// <summary>
    /// DxfLeader class
    /// </summary>
    public partial class DxfLeader : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Leader; } }
        protected override DxfAcadVersion MinVersion { get { return DxfAcadVersion.R13; } }

        public string DimensionStyleName { get; set; }
        public bool UseArrowheads { get; set; }
        public DxfLeaderPathType PathType { get; set; }
        public DxfLeaderCreationAnnotationType AnnotationType { get; set; }
        public DxfLeaderHooklineDirection HooklineDirection { get; set; }
        public bool UseHookline { get; set; }
        public double TextAnnotationHeight { get; set; }
        public double TextAnnotationWidth { get; set; }
        public int VertexCount { get; set; }
        private List<double> _verticesX { get; set; }
        private List<double> _verticesY { get; set; }
        private List<double> _verticesZ { get; set; }
        public DxfColor OverrideColor { get; set; }
        public string AssociatedAnnotationReference { get; set; }
        public DxfVector Normal { get; set; }
        public DxfVector Right { get; set; }
        public DxfVector BlockOffset { get; set; }
        public DxfVector AnnotationOffset { get; set; }
        public DxfXData XData { get { return XDataProtected; } set { XDataProtected = value; } }

        public DxfLeader()
            : base()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.DimensionStyleName = null;
            this.UseArrowheads = true;
            this.PathType = DxfLeaderPathType.StraightLineSegments;
            this.AnnotationType = DxfLeaderCreationAnnotationType.NoAnnotation;
            this.HooklineDirection = DxfLeaderHooklineDirection.OppositeFromHorizontalVector;
            this.UseHookline = true;
            this.TextAnnotationHeight = 1.0;
            this.TextAnnotationWidth = 1.0;
            this.VertexCount = 0;
            this._verticesX = new List<double>();
            this._verticesY = new List<double>();
            this._verticesZ = new List<double>();
            this.OverrideColor = DxfColor.ByBlock;
            this.AssociatedAnnotationReference = null;
            this.Normal = DxfVector.ZAxis;
            this.Right = DxfVector.XAxis;
            this.BlockOffset = DxfVector.Zero;
            this.AnnotationOffset = DxfVector.XAxis;
        }

        protected override void AddValuePairs(List<DxfCodePair> pairs, DxfAcadVersion version, bool outputHandles)
        {
            base.AddValuePairs(pairs, version, outputHandles);
            pairs.Add(new DxfCodePair(100, "AcDbLeader"));
            pairs.Add(new DxfCodePair(3, (this.DimensionStyleName)));
            pairs.Add(new DxfCodePair(71, BoolShort(this.UseArrowheads)));
            pairs.Add(new DxfCodePair(72, (short)(this.PathType)));
            pairs.Add(new DxfCodePair(73, (short)(this.AnnotationType)));
            pairs.Add(new DxfCodePair(74, (short)(this.HooklineDirection)));
            pairs.Add(new DxfCodePair(75, BoolShort(this.UseHookline)));
            pairs.Add(new DxfCodePair(40, (this.TextAnnotationHeight)));
            pairs.Add(new DxfCodePair(41, (this.TextAnnotationWidth)));
            pairs.Add(new DxfCodePair(76, (short)Vertices.Count));
            foreach (var item in Vertices)
            {
                pairs.Add(new DxfCodePair(10, item.X));
                pairs.Add(new DxfCodePair(20, item.Y));
                pairs.Add(new DxfCodePair(30, item.Z));
            }

            if (this.OverrideColor != DxfColor.ByBlock)
            {
                pairs.Add(new DxfCodePair(77, DxfColor.GetRawValue(this.OverrideColor)));
            }

            pairs.Add(new DxfCodePair(340, (this.AssociatedAnnotationReference)));
            pairs.Add(new DxfCodePair(210, Normal?.X ?? default(double)));
            pairs.Add(new DxfCodePair(220, Normal?.Y ?? default(double)));
            pairs.Add(new DxfCodePair(230, Normal?.Z ?? default(double)));
            pairs.Add(new DxfCodePair(211, Right?.X ?? default(double)));
            pairs.Add(new DxfCodePair(221, Right?.Y ?? default(double)));
            pairs.Add(new DxfCodePair(231, Right?.Z ?? default(double)));
            pairs.Add(new DxfCodePair(212, BlockOffset?.X ?? default(double)));
            pairs.Add(new DxfCodePair(222, BlockOffset?.Y ?? default(double)));
            pairs.Add(new DxfCodePair(232, BlockOffset?.Z ?? default(double)));
            if (version >= DxfAcadVersion.R14)
            {
                pairs.Add(new DxfCodePair(213, AnnotationOffset?.X ?? default(double)));
                pairs.Add(new DxfCodePair(223, AnnotationOffset?.Y ?? default(double)));
                pairs.Add(new DxfCodePair(233, AnnotationOffset?.Z ?? default(double)));
            }

            if (XData != null)
            {
                XData.AddValuePairs(pairs, version, outputHandles);
            }
        }

        internal override bool TrySetPair(DxfCodePair pair)
        {
            switch (pair.Code)
            {
                case 3:
                    this.DimensionStyleName = (pair.StringValue);
                    break;
                case 10:
                    this._verticesX.Add((pair.DoubleValue));
                    break;
                case 20:
                    this._verticesY.Add((pair.DoubleValue));
                    break;
                case 30:
                    this._verticesZ.Add((pair.DoubleValue));
                    break;
                case 40:
                    this.TextAnnotationHeight = (pair.DoubleValue);
                    break;
                case 41:
                    this.TextAnnotationWidth = (pair.DoubleValue);
                    break;
                case 71:
                    this.UseArrowheads = BoolShort(pair.ShortValue);
                    break;
                case 72:
                    this.PathType = (DxfLeaderPathType)(pair.ShortValue);
                    break;
                case 73:
                    this.AnnotationType = (DxfLeaderCreationAnnotationType)(pair.ShortValue);
                    break;
                case 74:
                    this.HooklineDirection = (DxfLeaderHooklineDirection)(pair.ShortValue);
                    break;
                case 75:
                    this.UseHookline = BoolShort(pair.ShortValue);
                    break;
                case 76:
                    this.VertexCount = (int)(pair.ShortValue);
                    break;
                case 77:
                    this.OverrideColor = DxfColor.FromRawValue(pair.ShortValue);
                    break;
                case 210:
                    this.Normal.X = pair.DoubleValue;
                    break;
                case 220:
                    this.Normal.Y = pair.DoubleValue;
                    break;
                case 230:
                    this.Normal.Z = pair.DoubleValue;
                    break;
                case 211:
                    this.Right.X = pair.DoubleValue;
                    break;
                case 221:
                    this.Right.Y = pair.DoubleValue;
                    break;
                case 231:
                    this.Right.Z = pair.DoubleValue;
                    break;
                case 212:
                    this.BlockOffset.X = pair.DoubleValue;
                    break;
                case 222:
                    this.BlockOffset.Y = pair.DoubleValue;
                    break;
                case 232:
                    this.BlockOffset.Z = pair.DoubleValue;
                    break;
                case 213:
                    this.AnnotationOffset.X = pair.DoubleValue;
                    break;
                case 223:
                    this.AnnotationOffset.Y = pair.DoubleValue;
                    break;
                case 233:
                    this.AnnotationOffset.Z = pair.DoubleValue;
                    break;
                case 340:
                    this.AssociatedAnnotationReference = (pair.StringValue);
                    break;
                default:
                    return base.TrySetPair(pair);
            }

            return true;
        }
    }

}
