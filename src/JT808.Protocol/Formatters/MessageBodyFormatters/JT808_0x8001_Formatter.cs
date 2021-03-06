﻿using JT808.Protocol.Enums;
using JT808.Protocol.Extensions;
using JT808.Protocol.MessageBody;
using JT808.Protocol.Interfaces;
using System;
using JT808.Protocol.MessagePack;

namespace JT808.Protocol.Formatters.MessageBodyFormatters
{
    public class JT808_0x8001_Formatter : IJT808MessagePackFormatter<JT808_0x8001>
    {
        public JT808_0x8001 Deserialize(ref JT808MessagePackReader reader, IJT808Config config)
        {
            JT808_0x8001 jT808_0X8001 = new JT808_0x8001();
            jT808_0X8001.MsgNum = reader.ReadUInt16();
            jT808_0X8001.MsgId = reader.ReadUInt16();
            jT808_0X8001.JT808PlatformResult = (JT808PlatformResult)reader.ReadByte();
            return jT808_0X8001;
        }

        public void Serialize(ref JT808MessagePackWriter writer, JT808_0x8001 value, IJT808Config config)
        {
            writer.WriteUInt16(value.MsgNum);
            writer.WriteUInt16(value.MsgId);
            writer.WriteByte((byte)value.JT808PlatformResult);
        }
    }
}
