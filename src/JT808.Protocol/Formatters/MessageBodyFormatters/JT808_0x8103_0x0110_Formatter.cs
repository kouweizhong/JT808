﻿using JT808.Protocol.Extensions;
using JT808.Protocol.MessageBody;
using JT808.Protocol.Interfaces;
using System;
using JT808.Protocol.MessagePack;

namespace JT808.Protocol.Formatters.MessageBodyFormatters
{
    public class JT808_0x8103_0x0110_Formatter : IJT808MessagePackFormatter<JT808_0x8103_0x0110>
    {
        public JT808_0x8103_0x0110 Deserialize(ref JT808MessagePackReader reader, IJT808Config config)
        {
            JT808_0x8103_0x0110 jT808_0x8103_0x0110 = new JT808_0x8103_0x0110();
            jT808_0x8103_0x0110.ParamId = reader.ReadUInt32();
            jT808_0x8103_0x0110.ParamLength = reader.ReadByte();
            jT808_0x8103_0x0110.ParamValue = reader.ReadArray(jT808_0x8103_0x0110.ParamLength).ToArray();
            return jT808_0x8103_0x0110;
        }

        public void Serialize(ref JT808MessagePackWriter writer, JT808_0x8103_0x0110 value, IJT808Config config)
        {
            writer.WriteUInt32(value.ParamId);
            writer.WriteByte((byte)value.ParamValue.Length);
            writer.WriteArray(value.ParamValue);
        }
    }
}