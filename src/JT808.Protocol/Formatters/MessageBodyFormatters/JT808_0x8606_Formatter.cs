﻿using JT808.Protocol.Extensions;
using JT808.Protocol.Metadata;
using JT808.Protocol.MessageBody;
using JT808.Protocol.Interfaces;
using System;
using System.Collections.Generic;
using JT808.Protocol.MessagePack;

namespace JT808.Protocol.Formatters.MessageBodyFormatters
{
    public class JT808_0x8606_Formatter : IJT808MessagePackFormatter<JT808_0x8606>
    {
        public JT808_0x8606 Deserialize(ref JT808MessagePackReader reader, IJT808Config config)
        {
            JT808_0x8606 jT808_0X8606 = new JT808_0x8606();
            jT808_0X8606.RouteId = reader.ReadUInt32();
            jT808_0X8606.RouteProperty = reader.ReadUInt16();
            ReadOnlySpan<char> routeProperty16Bit = Convert.ToString(jT808_0X8606.RouteProperty, 2).PadLeft(16, '0').AsSpan();
            bool bit0Flag = routeProperty16Bit.Slice(routeProperty16Bit.Length - 1).ToString().Equals("0");
            if (!bit0Flag)
            {
                jT808_0X8606.StartTime = reader.ReadDateTime6();
                jT808_0X8606.EndTime = reader.ReadDateTime6();
            }
            jT808_0X8606.InflectionPointCount = reader.ReadUInt16();
            jT808_0X8606.InflectionPointItems = new List<JT808InflectionPointProperty>();
            for (var i = 0; i < jT808_0X8606.InflectionPointCount; i++)
            {
                JT808InflectionPointProperty jT808InflectionPointProperty = new JT808InflectionPointProperty();
                jT808InflectionPointProperty.InflectionPointId = reader.ReadUInt32();
                jT808InflectionPointProperty.SectionId = reader.ReadUInt32();
                jT808InflectionPointProperty.InflectionPointLat = reader.ReadUInt32();
                jT808InflectionPointProperty.InflectionPointLng = reader.ReadUInt32();
                jT808InflectionPointProperty.SectionWidth = reader.ReadByte();
                jT808InflectionPointProperty.SectionProperty = reader.ReadByte();
                ReadOnlySpan<char> sectionProperty16Bit = Convert.ToString(jT808InflectionPointProperty.SectionProperty, 2).PadLeft(16, '0').AsSpan();
                bool sectionBit0Flag = sectionProperty16Bit.Slice(sectionProperty16Bit.Length - 1).ToString().Equals("0");
                if (!sectionBit0Flag)
                {
                    jT808InflectionPointProperty.SectionLongDrivingThreshold = reader.ReadUInt16();
                    jT808InflectionPointProperty.SectionDrivingUnderThreshold = reader.ReadUInt16();
                }
                bool sectionBit1Flag = sectionProperty16Bit.Slice(sectionProperty16Bit.Length - 2, 1).ToString().Equals("0");
                if (!sectionBit1Flag)
                {
                    jT808InflectionPointProperty.SectionHighestSpeed = reader.ReadUInt16();
                    jT808InflectionPointProperty.SectionOverspeedDuration = reader.ReadByte();
                }
                jT808_0X8606.InflectionPointItems.Add(jT808InflectionPointProperty);
            }
            return jT808_0X8606;
        }

        public void Serialize(ref JT808MessagePackWriter writer, JT808_0x8606 value, IJT808Config config)
        {
            writer.WriteUInt32(value.RouteId);
            writer.WriteUInt16(value.RouteProperty);
            ReadOnlySpan<char> routeProperty16Bit = Convert.ToString(value.RouteProperty, 2).PadLeft(16, '0').AsSpan();
            bool bit0Flag = routeProperty16Bit.Slice(routeProperty16Bit.Length - 1).ToString().Equals("0");
            if (!bit0Flag)
            {
                if (value.StartTime.HasValue)
                    writer.WriteDateTime6(value.StartTime.Value);

                if (value.EndTime.HasValue)
                    writer.WriteDateTime6(value.EndTime.Value);
            }
            //bool bit1Flag = routeProperty16Bit.Slice(routeProperty16Bit.Length - 2, 1).ToString().Equals("0");
            if (value.InflectionPointItems != null && value.InflectionPointItems.Count > 0)
            {
                writer.WriteUInt16((ushort)value.InflectionPointItems.Count);
                foreach (var item in value.InflectionPointItems)
                {
                   writer.WriteUInt32( item.InflectionPointId);
                   writer.WriteUInt32( item.SectionId);
                   writer.WriteUInt32(item.InflectionPointLat);
                   writer.WriteUInt32(item.InflectionPointLng);
                   writer.WriteByte(item.SectionWidth);
                    writer.WriteByte(item.SectionProperty);

                    ReadOnlySpan<char> sectionProperty16Bit = Convert.ToString(item.SectionProperty, 2).PadLeft(16, '0').AsSpan();
                    bool sectionBit0Flag = sectionProperty16Bit.Slice(sectionProperty16Bit.Length - 1).ToString().Equals("0");
                    if (!sectionBit0Flag)
                    {
                        if (item.SectionLongDrivingThreshold.HasValue)
                            writer.WriteUInt16(item.SectionLongDrivingThreshold.Value);
                        if (item.SectionDrivingUnderThreshold.HasValue)
                            writer.WriteUInt16(item.SectionDrivingUnderThreshold.Value);
                    }
                    bool sectionBit1Flag = sectionProperty16Bit.Slice(sectionProperty16Bit.Length - 2, 1).ToString().Equals("0");
                    if (!sectionBit1Flag)
                    {
                        if (item.SectionHighestSpeed.HasValue)
                            writer.WriteUInt16(item.SectionHighestSpeed.Value);
                        if (item.SectionOverspeedDuration.HasValue)
                            writer.WriteByte(item.SectionOverspeedDuration.Value);
                    }
                }
            }
        }
    }
}
