﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using IxMilia.Dxf.Objects;
using Xunit;

namespace IxMilia.Dxf.Test
{
    public class DxfObjectTests : AbstractDxfTests
    {
        private DxfObject GenObject(string typeString, string contents)
        {
            return Section("OBJECTS", $@"
  0
{typeString}
{contents.Trim()}
").Objects.Single();
        }

        [Fact]
        public void ReadSimpleObjectTest()
        {
            var proxyObject = GenObject("ACAD_PROXY_OBJECT", "");
            Assert.IsType<DxfAcadProxyObject>(proxyObject);
        }

        [Fact]
        public void WriteSimpleObjectTest()
        {
            var file = new DxfFile();
            file.Header.Version = DxfAcadVersion.R2000;
            file.Objects.Add(new DxfAcadProxyObject());
            VerifyFileContains(file, @"
  0
ACAD_PROXY_OBJECT
  5
A
330
0
100
AcDbProxyObject
 90
499
");
        }

        [Fact]
        public void ReadDataTableTest()
        {
            var table = (DxfDataTable)GenObject("DATATABLE", @"
 90
2
 91
2
  1
table-name
 92
10
  2
column-of-points
 10
1
 20
2
 30
3
 10
4
 20
5
 30
6
 92
3
  2
column-of-strings
  3
string 1
  3
string 2
");
            Assert.Equal(2, table.ColumnCount);
            Assert.Equal(2, table.RowCount);
            Assert.Equal("table-name", table.Name);

            Assert.Equal("column-of-points", table.ColumnNames[0]);
            Assert.Equal(new DxfPoint(1, 2, 3), (DxfPoint)table[0, 0]);
            Assert.Equal(new DxfPoint(4, 5, 6), (DxfPoint)table[1, 0]);

            Assert.Equal("column-of-strings", table.ColumnNames[1]);
            Assert.Equal("string 1", (string)table[0, 1]);
            Assert.Equal("string 2", (string)table[1, 1]);
        }

        [Fact]
        public void WriteDataTableTest()
        {
            var table = new DxfDataTable();
            table.Name = "table-name";
            table.SetSize(2, 2);
            table.ColumnNames.Add("column-of-points");
            table.ColumnNames.Add("column-of-strings");
            table[0, 0] = new DxfPoint(1, 2, 3);
            table[1, 0] = new DxfPoint(4, 5, 6);
            table[0, 1] = "string 1";
            table[1, 1] = "string 2";
            var file = new DxfFile();
            file.Header.Version = DxfAcadVersion.R2007;
            file.Objects.Add(table);
            VerifyFileContains(file, @"
  0
DATATABLE
  5
A
330
0
100
AcDbDataTable
 70
0
 90
2
 91
2
  1
table-name
 92
10
  2
column-of-points
 10
1.0
 20
2.0
 30
3.0
 10
4.0
 20
5.0
 30
6.0
 92
3
  2
column-of-strings
  3
string 1
  3
string 2
");
        }

        [Fact]
        public void ReadDictionaryTest()
        {
            var dict = (DxfDictionary)GenObject("DICTIONARY", @"
100
AcDbDictionary
280
1
281
1
  3
name-1
350
1
  3
name-2
350
2
  3
name-3
350
3
");
            Assert.True(dict.IsHardOwner);
            Assert.Equal(DxfDictionaryDuplicateRecordHandling.KeepExisting, dict.DuplicateRecordHandling);
            Assert.Equal(3, dict.Count);
            Assert.Equal(1u, dict["name-1"]);
            Assert.Equal(2u, dict["name-2"]);
            Assert.Equal(3u, dict["name-3"]);
        }

        [Fact]
        public void WriteDictionaryTest()
        {
            var dict = new DxfDictionary();
            dict["name-1"] = 1u;
            dict["name-2"] = 2u;
            dict["name-3"] = 3u;
            dict.IsHardOwner = true;
            dict.DuplicateRecordHandling = DxfDictionaryDuplicateRecordHandling.KeepExisting;
            var file = new DxfFile();
            file.Objects.Add(dict);
            VerifyFileContains(file, @"
  0
DICTIONARY
  5
A
100
AcDbDictionary
  3
name-1
350
1
  3
name-2
350
2
  3
name-3
350
3
");
        }

        [Fact]
        public void ReadLayoutTest()
        {
            var layout = (DxfLayout)GenObject("LAYOUT", @"
  1
page-setup-name
100
AcDbLayout
  1
layout-name
");
            Assert.Equal("page-setup-name", layout.PageSetupName);
            Assert.Equal("layout-name", layout.LayoutName);
        }

        [Fact]
        public void WriteLayoutTest()
        {
            var layout = new DxfLayout();
            layout.PageSetupName = "page-setup-name";
            layout.LayoutName = "layout-name";
            var file = new DxfFile();
            file.Objects.Add(layout);
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms))
                {
                    var contents = reader.ReadToEnd();

                    // verify the plot settings were written
                    var plotSettingsOffset = contents.IndexOf(@"
100
AcDbPlotSettings
  1
page-setup-name
".Trim());
                    Assert.True(plotSettingsOffset > 0);

                    // verify the layout settings were written
                    var layoutOffset = contents.IndexOf(@"
100
AcDbLayout
  1
layout-name
".Trim());
                    Assert.True(layoutOffset > 0);

                    // verify that the layout settings were written after the plot settings
                    Assert.True(layoutOffset > plotSettingsOffset);
                }
            }
        }

        [Fact]
        public void ReadLightListTest()
        {
            var lightList = (DxfLightList)GenObject("LIGHTLIST", @"
 90
42
 90
3
  5
111
  1
uno
  5
222
  1
dos
  5
333
  1
tres
");
            Assert.Equal(42, lightList.Version);
            Assert.Equal(3, lightList.Lights.Count);

            Assert.Equal(0x111u, lightList.Lights[0].Handle);
            Assert.Equal("uno", lightList.Lights[0].Name);
            Assert.Equal(0x222u, lightList.Lights[1].Handle);
            Assert.Equal("dos", lightList.Lights[1].Name);
            Assert.Equal(0x333u, lightList.Lights[2].Handle);
            Assert.Equal("tres", lightList.Lights[2].Name);
        }

        [Fact]
        public void WriteLightListTest()
        {
            var lightList = new DxfLightList();
            lightList.Version = 42;
            lightList.Lights.Add(new DxfLightList.DxfLightListItem() { Handle = 0x111, Name = "uno" });
            lightList.Lights.Add(new DxfLightList.DxfLightListItem() { Handle = 0x222, Name = "dos" });
            lightList.Lights.Add(new DxfLightList.DxfLightListItem() { Handle = 0x333, Name = "tres" });
            var file = new DxfFile();
            file.Objects.Add(lightList);
            VerifyFileContains(file, @"
 90
42
 90
3
  5
111
  1
uno
  5
222
  1
dos
  5
333
  1
tres
");
        }

        [Fact]
        public void ReadMaterialTest()
        {
            var material = (DxfMaterial)GenObject("MATERIAL", @"
 75
1
 43
1
 43
2
 43
3
 43
4
 43
5
 43
6
 43
7
 43
8
 43
9
 43
10
 43
11
 43
12
 43
13
 43
14
 43
15
 43
16
 75
2
 43
10
 43
20
 43
30
 43
40
 43
50
 43
60
 43
70
 43
80
 43
90
 43
100
 43
110
 43
120
 43
130
 43
140
 43
150
 43
160
");
            Assert.Equal(DxfMapAutoTransformMethod.NoAutoTransform, material.DiffuseMapAutoTransformMethod);
            Assert.Equal(1.0, material.DiffuseMapTransformMatrix.M11);
            Assert.Equal(2.0, material.DiffuseMapTransformMatrix.M12);
            Assert.Equal(3.0, material.DiffuseMapTransformMatrix.M13);
            Assert.Equal(4.0, material.DiffuseMapTransformMatrix.M14);
            Assert.Equal(5.0, material.DiffuseMapTransformMatrix.M21);
            Assert.Equal(6.0, material.DiffuseMapTransformMatrix.M22);
            Assert.Equal(7.0, material.DiffuseMapTransformMatrix.M23);
            Assert.Equal(8.0, material.DiffuseMapTransformMatrix.M24);
            Assert.Equal(9.0, material.DiffuseMapTransformMatrix.M31);
            Assert.Equal(10.0, material.DiffuseMapTransformMatrix.M32);
            Assert.Equal(11.0, material.DiffuseMapTransformMatrix.M33);
            Assert.Equal(12.0, material.DiffuseMapTransformMatrix.M34);
            Assert.Equal(13.0, material.DiffuseMapTransformMatrix.M41);
            Assert.Equal(14.0, material.DiffuseMapTransformMatrix.M42);
            Assert.Equal(15.0, material.DiffuseMapTransformMatrix.M43);
            Assert.Equal(16.0, material.DiffuseMapTransformMatrix.M44);

            Assert.Equal(DxfMapAutoTransformMethod.ScaleToCurrentEntity, material.NormalMapAutoTransformMethod);
            Assert.Equal(10.0, material.NormalMapTransformMatrix.M11);
            Assert.Equal(20.0, material.NormalMapTransformMatrix.M12);
            Assert.Equal(30.0, material.NormalMapTransformMatrix.M13);
            Assert.Equal(40.0, material.NormalMapTransformMatrix.M14);
            Assert.Equal(50.0, material.NormalMapTransformMatrix.M21);
            Assert.Equal(60.0, material.NormalMapTransformMatrix.M22);
            Assert.Equal(70.0, material.NormalMapTransformMatrix.M23);
            Assert.Equal(80.0, material.NormalMapTransformMatrix.M24);
            Assert.Equal(90.0, material.NormalMapTransformMatrix.M31);
            Assert.Equal(100.0, material.NormalMapTransformMatrix.M32);
            Assert.Equal(110.0, material.NormalMapTransformMatrix.M33);
            Assert.Equal(120.0, material.NormalMapTransformMatrix.M34);
            Assert.Equal(130.0, material.NormalMapTransformMatrix.M41);
            Assert.Equal(140.0, material.NormalMapTransformMatrix.M42);
            Assert.Equal(150.0, material.NormalMapTransformMatrix.M43);
            Assert.Equal(160.0, material.NormalMapTransformMatrix.M44);
        }

        [Fact]
        public void ReadMLineStyleTest()
        {
            var mlineStyle = (DxfMLineStyle)GenObject("MLINESTYLE", @"
  2
<name>
  3
<description>
 62
1
 51
99.0
 52
100.0
 71
2
 49
3.0
 62
3
  6
tres
 49
4.0
 62
4
  6
quatro
");
            Assert.Equal("<name>", mlineStyle.StyleName);
            Assert.Equal("<description>", mlineStyle.Description);
            Assert.Equal(1, mlineStyle.FillColor.RawValue);
            Assert.Equal(99.0, mlineStyle.StartAngle);
            Assert.Equal(100.0, mlineStyle.EndAngle);
            Assert.Equal(2, mlineStyle.Elements.Count);

            Assert.Equal(3.0, mlineStyle.Elements[0].Offset);
            Assert.Equal(3, mlineStyle.Elements[0].Color.RawValue);
            Assert.Equal("tres", mlineStyle.Elements[0].Linetype);

            Assert.Equal(4.0, mlineStyle.Elements[1].Offset);
            Assert.Equal(4, mlineStyle.Elements[1].Color.RawValue);
            Assert.Equal("quatro", mlineStyle.Elements[1].Linetype);
        }

        [Fact]
        public void WriteMLineStyleTest()
        {
            var mlineStyle = new DxfMLineStyle();
            mlineStyle.StyleName = "<name>";
            mlineStyle.Description = "<description>";
            mlineStyle.FillColor = DxfColor.FromRawValue(1);
            mlineStyle.StartAngle = 99.9;
            mlineStyle.EndAngle = 100.0;
            mlineStyle.Elements.Add(new DxfMLineStyle.DxfMLineStyleElement() { Offset = 3.0, Color = DxfColor.FromRawValue(3), Linetype = "tres" });
            mlineStyle.Elements.Add(new DxfMLineStyle.DxfMLineStyleElement() { Offset = 4.0, Color = DxfColor.FromRawValue(4), Linetype = "quatro" });
            var file = new DxfFile();
            file.Objects.Add(mlineStyle);
            VerifyFileContains(file, @"
100
AcDbMlineStyle
  2
<name>
 70
0
  3
<description>
 62
1
 51
99.9
 52
100.0
 71
2
 49
3.0
 62
3
  6
tres
 49
4.0
 62
4
  6
quatro
");
        }

        [Fact]
        public void ReadSectionSettingsTest()
        {
            var settings = (DxfSectionSettings)GenObject("SECTIONSETTINGS", @"
  5
A
100
AcDbSectionSettings
 90
42
 91
1
  1
SectionTypeSettings
 90
43
 91
1
 92
2
330
100
330
101
331
FF
  1
file-name
 93
2
  2
SectionGeometrySettings
 90
1001
 91
0
 92
0
 63
0
  8

  6

 40
1.0
  1

370
0
 70
0
 71
0
 72
0
  2

 41
0.0
 42
1.0
 43
0.0
  3
SectionGeometrySettingsEnd
 90
1002
 91
0
 92
0
 63
0
  8

  6

 40
1.0
  1

370
0
 70
0
 71
0
 72
0
  2

 41
0.0
 42
1.0
 43
0.0
  3
SectionGeometrySettingsEnd
  3
SectionTypeSettingsEnd
");
            Assert.Equal(42, settings.SectionType);
            var typeSettings = settings.SectionTypeSettings.Single();
            Assert.Equal(43, typeSettings.SectionType);
            Assert.True(typeSettings.IsGenerationOption);
            Assert.Equal(0xFFu, typeSettings.DestinationObjectHandle);
            Assert.Equal("file-name", typeSettings.DestinationFileName);

            Assert.Equal(2, typeSettings.SourceObjectHandles.Count);
            Assert.Equal(0x100u, typeSettings.SourceObjectHandles[0]);
            Assert.Equal(0x101u, typeSettings.SourceObjectHandles[1]);

            Assert.Equal(2, typeSettings.GeometrySettings.Count);
            Assert.Equal(1001, typeSettings.GeometrySettings[0].SectionType);
            Assert.Equal(1002, typeSettings.GeometrySettings[1].SectionType);
        }

        [Fact]
        public void WriteSectionSettingsTest()
        {
            var settings = new DxfSectionSettings();
            settings.SectionType = 42;
            var typeSettings = new DxfSectionTypeSettings()
            {
                SectionType = 43,
                IsGenerationOption = true,
                DestinationObjectHandle = 0xFFu,
                DestinationFileName = "file-name",
            };
            typeSettings.SourceObjectHandles.Add(0x100u);
            typeSettings.SourceObjectHandles.Add(0x101u);
            typeSettings.GeometrySettings.Add(new DxfSectionGeometrySettings() { SectionType = 1001 });
            typeSettings.GeometrySettings.Add(new DxfSectionGeometrySettings() { SectionType = 1002 });
            settings.SectionTypeSettings.Add(typeSettings);
            var file = new DxfFile();
            file.Objects.Add(settings);
            VerifyFileContains(file, @"
  0
SECTIONSETTINGS
  5
A
100
AcDbSectionSettings
 90
42
 91
1
  1
SectionTypeSettings
 90
43
 91
1
 92
2
330
100
330
101
331
FF
  1
file-name
 93
2
  2
SectionGeometrySettings
 90
1001
 91
0
 92
0
 63
0
  8

  6

 40
1.0
  1

370
0
 70
0
 71
0
 72
0
  2

 41
0.0
 42
1.0
 43
0.0
  3
SectionGeometrySettingsEnd
 90
1002
 91
0
 92
0
 63
0
  8

  6

 40
1.0
  1

370
0
 70
0
 71
0
 72
0
  2

 41
0.0
 42
1.0
 43
0.0
  3
SectionGeometrySettingsEnd
  3
SectionTypeSettingsEnd
");
        }

        [Fact]
        public void ReadSortentsTableTest()
        {
            var sortents = (DxfSortentsTable)GenObject("SORTENTSTABLE", @"
  5
A
100
AcDbSortentsTable
331
2000
331
2001
331
2002
  5
3000
  5
3001
  5
3002
");
            Assert.Equal(0xAu, sortents.Handle);
            Assert.Equal(3, sortents.EntityHandles.Count);
            Assert.Equal(0x2000u, sortents.EntityHandles[0]);
            Assert.Equal(0x2001u, sortents.EntityHandles[1]);
            Assert.Equal(0x2002u, sortents.EntityHandles[2]);
            Assert.Equal(0x3000u, sortents.SortHandles[0]);
            Assert.Equal(0x3001u, sortents.SortHandles[1]);
            Assert.Equal(0x3002u, sortents.SortHandles[2]);
        }

        [Fact]
        public void WriteSortentsTableTest()
        {
            var sortents = new DxfSortentsTable();
            sortents.EntityHandles.Add(0x2000u);
            sortents.EntityHandles.Add(0x2001u);
            sortents.EntityHandles.Add(0x2002u);
            sortents.SortHandles.Add(0x3000u);
            sortents.SortHandles.Add(0x3001u);
            sortents.SortHandles.Add(0x3002u);
            var file = new DxfFile();
            file.Objects.Add(sortents);
            VerifyFileContains(file, @"
  0
SORTENTSTABLE
  5
A
100
AcDbSortentsTable
331
2000
331
2001
331
2002
  5
3000
  5
3001
  5
3002
");
        }

        [Fact]
        public void ReadSunStudyTest()
        {
            // with subset
            var sun = (DxfSunStudy)GenObject("SUNSTUDY", @"
290
1
 73
3
290
42
290
43
290
44
");
            Assert.True(sun.UseSubset);
            Assert.Equal(3, sun.Hours.Count);
            Assert.Equal(42, sun.Hours[0]);
            Assert.Equal(43, sun.Hours[1]);
            Assert.Equal(44, sun.Hours[2]);

            // without subset
            sun = (DxfSunStudy)GenObject("SUNSTUDY", @"
 73
3
290
42
290
43
290
44
");
            Assert.False(sun.UseSubset);
            Assert.Equal(3, sun.Hours.Count);
            Assert.Equal(42, sun.Hours[0]);
            Assert.Equal(43, sun.Hours[1]);
            Assert.Equal(44, sun.Hours[2]);
        }

        [Fact]
        public void WriteSunStudyTest()
        {
            var sun = new DxfSunStudy();
            sun.Hours.Add(42);
            sun.Hours.Add(43);
            sun.Hours.Add(44);
            var file = new DxfFile();
            file.Objects.Add(sun);
            VerifyFileContains(file, @"
  0
SUNSTUDY
  5
A
100
AcDbSunStudy
 90
0
  1

  2

 70
0
  3

290
0
  4

291
0
 91
0
292
0
 73
3
290
42
290
43
290
44
340
0
341
0
342
0
 74
0
 75
0
 76
0
 77
0
 40
0.0
293
0
294
0
343
0
");

            // verify writing as binary doesn't crash
            using (var ms = new MemoryStream())
            {
                file.Save(ms, asText: false);
            }
        }

        [Fact]
        public void ReadTableStyleTest()
        {
            var table = (DxfTableStyle)GenObject("TABLESTYLE", @"
  5
A
100
AcDbTableStyle
280
0
  3

 70
0
 71
0
 40
0.06
 41
0.06
280
0
281
0
  7
one
140
0.0
170
0
 62
0
 63
7
283
0
 90
0
 91
0
274
0
275
0
276
0
277
0
278
0
279
0
284
1
285
1
286
1
287
1
288
1
289
1
 64
0
 65
0
 66
0
 67
0
 68
0
 69
0
  7
two
140
0.0
170
0
 62
0
 63
7
283
0
 90
0
 91
0
274
0
275
0
276
0
277
0
278
0
279
0
284
1
285
1
286
1
287
1
288
1
289
1
 64
0
 65
0
 66
0
 67
0
 68
0
 69
0
");
            Assert.Equal(2, table.CellStyles.Count);
            Assert.Equal("one", table.CellStyles[0].Name);
            Assert.Equal("two", table.CellStyles[1].Name);
        }

        [Fact]
        public void WriteTableStyleTest()
        {
            var table = new DxfTableStyle();
            table.CellStyles.Add(new DxfTableCellStyle() { Name = "one" });
            table.CellStyles.Add(new DxfTableCellStyle() { Name = "two" });
            var file = new DxfFile();
            file.Objects.Add(table);
            VerifyFileContains(file, @"
  0
TABLESTYLE
  5
A
100
AcDbTableStyle
  3

 70
0
 71
0
 40
0.06
 41
0.06
280
0
281
0
  7
one
140
0.0
170
0
 62
0
 63
7
283
0
 90
0
 91
0
274
0
275
0
276
0
277
0
278
0
279
0
284
1
285
1
286
1
287
1
288
1
289
1
 64
0
 65
0
 66
0
 67
0
 68
0
 69
0
  7
two
140
0.0
170
0
 62
0
 63
7
283
0
 90
0
 91
0
274
0
275
0
276
0
277
0
278
0
279
0
284
1
285
1
286
1
287
1
288
1
289
1
 64
0
 65
0
 66
0
 67
0
 68
0
 69
0
");
        }

        [Fact]
        public void WriteAllDefaultObjectsTest()
        {
            var file = new DxfFile();
            var assembly = typeof(DxfFile).Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (IsDxfObjectOrDerived(type))
                {
                    var ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor != null)
                    {
                        // add the object with its default initialized values
                        var obj = (DxfObject)ctor.Invoke(new object[0]);
                        file.Objects.Add(obj);

                        // add the entity with its default values and 2 items added to each List<T> collection
                        obj = (DxfObject)ctor.Invoke(new object[0]);
                        foreach (var property in type.GetProperties().Where(p => IsListOfT(p.PropertyType)))
                        {
                            var itemType = property.PropertyType.GenericTypeArguments.Single();
                            var itemValue = itemType.IsValueType
                                ? Activator.CreateInstance(itemType)
                                : null;
                            var addMethod = property.PropertyType.GetMethod("Add");
                            var propertyValue = property.GetValue(obj);
                            addMethod.Invoke(propertyValue, new object[] { itemValue });
                            addMethod.Invoke(propertyValue, new object[] { itemValue });
                        }

                        // add an object with all non-indexer properties set to `default(T)`
                        obj = (DxfObject)SetAllPropertiesToDefault(ctor.Invoke(new object[0]));
                        file.Objects.Add(obj);
                    }
                }
            }

            // write each version of the objects with default versions
            foreach (var version in new[] { DxfAcadVersion.R10, DxfAcadVersion.R11, DxfAcadVersion.R12, DxfAcadVersion.R13, DxfAcadVersion.R14, DxfAcadVersion.R2000, DxfAcadVersion.R2004, DxfAcadVersion.R2007, DxfAcadVersion.R2010, DxfAcadVersion.R2013 })
            {
                file.Header.Version = version;
                using (var ms = new MemoryStream())
                {
                    file.Save(ms);
                }
            }
        }

        private static bool IsDxfObjectOrDerived(Type type)
        {
            if (type == typeof(DxfObject))
            {
                return true;
            }

            if (type.BaseType != null)
            {
                return IsDxfObjectOrDerived(type.BaseType);
            }

            return false;
        }
    }
}
