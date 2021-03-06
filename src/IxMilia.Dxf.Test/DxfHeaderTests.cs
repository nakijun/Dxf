﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Xunit;

namespace IxMilia.Dxf.Test
{
    public class DxfHeaderTests : AbstractDxfTests
    {
        #region Read tests

        [Fact]
        public void SpecificHeaderValuesTest()
        {
            var file = Section("HEADER", @"
  9
$ACADMAINTVER
 70
16
  9
$ACADVER
  1
AC1012
  9
$ANGBASE
 50
5.5E1
  9
$ANGDIR
 70
1
  9
$ATTMODE
 70
1
  9
$AUNITS
 70
3
  9
$AUPREC
 70
7
  9
$CLAYER
  8
<current layer>
  9
$LUNITS
 70
6
  9
$LUPREC
 70
7
");
            Assert.Equal(16, file.Header.MaintenenceVersion);
            Assert.Equal(DxfAcadVersion.R13, file.Header.Version);
            Assert.Equal(55.0, file.Header.AngleZeroDirection);
            Assert.Equal(DxfAngleDirection.Clockwise, file.Header.AngleDirection);
            Assert.Equal(DxfAttributeVisibility.Normal, file.Header.AttributeVisibility);
            Assert.Equal(DxfAngleFormat.Radians, file.Header.AngleUnitFormat);
            Assert.Equal(7, file.Header.AngleUnitPrecision);
            Assert.Equal("<current layer>", file.Header.CurrentLayer);
            Assert.Equal(DxfUnitFormat.Architectural, file.Header.UnitFormat);
            Assert.Equal(7, file.Header.UnitPrecision);
        }

        [Fact]
        public void DateConversionTest()
        {
            // from Autodesk spec: 2451544.91568287 = December 31, 1999, 9:58:35 pm.

            // verify reading
            var file = Section("HEADER", @"
  9
$TDCREATE
 40
2451544.91568287
");
            Assert.Equal(new DateTime(1999, 12, 31, 21, 58, 35), file.Header.CreationDate);

            VerifyFileContains(file, @"
  9
$TDCREATE
 40
2451544.91568287
");
        }

        [Fact]
        public void ReadLayerTableTest()
        {
            var file = Section("TABLES", @"
  0
TABLE
  2
LAYER
  0
LAYER
  2
a
 62
12
  0
LAYER
102
{APP_NAME
  1
foo
  2
bar
102
}
  2
b
 62
13
  0
ENDTAB
");
            var layers = file.Layers;
            Assert.Equal(2, layers.Count);
            Assert.Equal("a", layers[0].Name);
            Assert.Equal(12, layers[0].Color.RawValue);
            Assert.Equal("b", layers[1].Name);
            Assert.Equal(13, layers[1].Color.RawValue);

            var group = layers[1].ExtensionDataGroups.Single();
            Assert.Equal("APP_NAME", group.GroupName);
            Assert.Equal(2, group.Items.Count);
            Assert.Equal(new DxfCodePair(1, "foo"), group.Items[0]);
            Assert.Equal(new DxfCodePair(2, "bar"), group.Items[1]);
        }

        [Fact]
        public void WriteLayersTableTest()
        {
            var layer = new DxfLayer("layer-name");
            layer.ExtensionDataGroups.Add(new DxfCodePairGroup("APP_NAME", new DxfCodePairOrGroup[]
            {
                new DxfCodePair(1, "foo"),
                new DxfCodePair(2, "bar"),
            }));
            var file = new DxfFile();
            file.Layers.Add(layer);
            VerifyFileContains(file, @"
  0
LAYER
  5
A
102
{APP_NAME
  1
foo
  2
bar
102
}
");
        }

        [Fact]
        public void ViewPortTableTest()
        {
            var file = Section("TABLES", @"
  0
TABLE
  2
VPORT
  0
VPORT
  0
VPORT
  2
vport-2
 10
1.100000E+001
 20
2.200000E+001
 11
3.300000E+001
 21
4.400000E+001
 12
5.500000E+001
 22
6.600000E+001
 13
7.700000E+001
 23
8.800000E+001
 14
9.900000E+001
 24
1.200000E+001
 15
1.300000E+001
 25
1.400000E+001
 16
1.500000E+001
 26
1.600000E+001
 36
1.700000E+001
 17
1.800000E+001
 27
1.900000E+001
 37
2.000000E+001
 40
2.100000E+001
 41
2.200000E+001
 42
2.300000E+001
 43
2.400000E+001
 44
2.500000E+001
 50
2.600000E+001
 51
2.700000E+001
  0
ENDTAB
");
            var viewPorts = file.ViewPorts;
            Assert.Equal(2, viewPorts.Count);

            // defaults
            Assert.Equal(null, viewPorts[0].Name);
            Assert.Equal(0.0, viewPorts[0].LowerLeft.X);
            Assert.Equal(0.0, viewPorts[0].LowerLeft.Y);
            Assert.Equal(0.0, viewPorts[0].UpperRight.X);
            Assert.Equal(0.0, viewPorts[0].UpperRight.Y);
            Assert.Equal(0.0, viewPorts[0].ViewCenter.X);
            Assert.Equal(0.0, viewPorts[0].ViewCenter.Y);
            Assert.Equal(0.0, viewPorts[0].SnapBasePoint.X);
            Assert.Equal(0.0, viewPorts[0].SnapBasePoint.Y);
            Assert.Equal(0.0, viewPorts[0].SnapSpacing.X);
            Assert.Equal(0.0, viewPorts[0].SnapSpacing.Y);
            Assert.Equal(0.0, viewPorts[0].GridSpacing.X);
            Assert.Equal(0.0, viewPorts[0].GridSpacing.Y);
            Assert.Equal(0.0, viewPorts[0].ViewDirection.X);
            Assert.Equal(0.0, viewPorts[0].ViewDirection.Y);
            Assert.Equal(1.0, viewPorts[0].ViewDirection.Z);
            Assert.Equal(0.0, viewPorts[0].TargetViewPoint.X);
            Assert.Equal(0.0, viewPorts[0].TargetViewPoint.Y);
            Assert.Equal(0.0, viewPorts[0].TargetViewPoint.Z);
            Assert.Equal(0.0, viewPorts[0].ViewHeight);
            Assert.Equal(0.0, viewPorts[0].ViewPortAspectRatio);
            Assert.Equal(0.0, viewPorts[0].LensLength);
            Assert.Equal(0.0, viewPorts[0].FrontClippingPlane);
            Assert.Equal(0.0, viewPorts[0].BackClippingPlane);
            Assert.Equal(0.0, viewPorts[0].SnapRotationAngle);
            Assert.Equal(0.0, viewPorts[0].ViewTwistAngle);

            // specifics
            Assert.Equal("vport-2", viewPorts[1].Name);
            Assert.Equal(11.0, viewPorts[1].LowerLeft.X);
            Assert.Equal(22.0, viewPorts[1].LowerLeft.Y);
            Assert.Equal(33.0, viewPorts[1].UpperRight.X);
            Assert.Equal(44.0, viewPorts[1].UpperRight.Y);
            Assert.Equal(55.0, viewPorts[1].ViewCenter.X);
            Assert.Equal(66.0, viewPorts[1].ViewCenter.Y);
            Assert.Equal(77.0, viewPorts[1].SnapBasePoint.X);
            Assert.Equal(88.0, viewPorts[1].SnapBasePoint.Y);
            Assert.Equal(99.0, viewPorts[1].SnapSpacing.X);
            Assert.Equal(12.0, viewPorts[1].SnapSpacing.Y);
            Assert.Equal(13.0, viewPorts[1].GridSpacing.X);
            Assert.Equal(14.0, viewPorts[1].GridSpacing.Y);
            Assert.Equal(15.0, viewPorts[1].ViewDirection.X);
            Assert.Equal(16.0, viewPorts[1].ViewDirection.Y);
            Assert.Equal(17.0, viewPorts[1].ViewDirection.Z);
            Assert.Equal(18.0, viewPorts[1].TargetViewPoint.X);
            Assert.Equal(19.0, viewPorts[1].TargetViewPoint.Y);
            Assert.Equal(20.0, viewPorts[1].TargetViewPoint.Z);
            Assert.Equal(21.0, viewPorts[1].ViewHeight);
            Assert.Equal(22.0, viewPorts[1].ViewPortAspectRatio);
            Assert.Equal(23.0, viewPorts[1].LensLength);
            Assert.Equal(24.0, viewPorts[1].FrontClippingPlane);
            Assert.Equal(25.0, viewPorts[1].BackClippingPlane);
            Assert.Equal(26.0, viewPorts[1].SnapRotationAngle);
            Assert.Equal(27.0, viewPorts[1].ViewTwistAngle);
        }

        #endregion

        #region Write tests

        [Fact]
        public void WriteDefaultHeaderValuesTest()
        {
            VerifyFileContains(new DxfFile(), @"
  9
$DIMGAP
 40
0.0
");
        }

        [Fact]
        public void WriteSpecificHeaderValuesTest()
        {
            var file = new DxfFile();
            file.Header.DimensionLineGap = 11.0;
            VerifyFileContains(file, @"
  9
$DIMGAP
 40
11.0
");
        }

        [Fact]
        public void WriteLayersTest()
        {
            var file = new DxfFile();
            file.Layers.Add(new DxfLayer("default"));
            VerifyFileContains(file, @"
  0
SECTION
  2
TABLES
  0
TABLE
  2
LAYER
  5
5
100
AcDbSymbolTable
 70
0
  0
LAYER
  5
A
100
AcDbSymbolTableRecord
100
AcDbLayerTableRecord
  2
default
 70
0
 62
1
  6

  0
ENDTAB
  0
ENDSEC
");
        }

        [Fact]
        public void WriteViewportTest()
        {
            var file = new DxfFile();
            file.ViewPorts.Add(new DxfViewPort());
            VerifyFileContains(file, @"
  0
SECTION
  2
TABLES
  0
TABLE
  2
VPORT
  5
9
100
AcDbSymbolTable
 70
0
  0
VPORT
  5
A
100
AcDbSymbolTableRecord
100
AcDbViewportTableRecord
  2

 70
0
 10
0.0
 20
0.0
 11
0.0
 21
0.0
 12
0.0
 22
0.0
 13
0.0
 23
0.0
 14
0.0
 24
0.0
 15
0.0
 25
0.0
 16
0.0
 26
0.0
 36
1.0
 17
0.0
 27
0.0
 37
0.0
 40
0.0
 41
0.0
 42
0.0
 43
0.0
 44
0.0
 50
0.0
 51
0.0
 71
0
 72
0
 73
1
 74
0
 75
0
 76
0
 77
0
 78
0
  0
ENDTAB
  0
ENDSEC
");
        }

        [Fact]
        public void WriteAndReadTypeDefaultsTest()
        {
            var file = new DxfFile();
            SetAllPropertiesToDefault(file.Header);

            // write each version of the header with default values
            foreach (var version in new[] { DxfAcadVersion.R10, DxfAcadVersion.R11, DxfAcadVersion.R12, DxfAcadVersion.R13, DxfAcadVersion.R14, DxfAcadVersion.R2000, DxfAcadVersion.R2004, DxfAcadVersion.R2007, DxfAcadVersion.R2010, DxfAcadVersion.R2013 })
            {
                file.Header.Version = version;
                using (var ms = new MemoryStream())
                {
                    file.Save(ms);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);

                    // ensure the header can be read back in, too
                    var file2 = DxfFile.Load(ms);
                }
            }
        }

        #endregion

    }
}
