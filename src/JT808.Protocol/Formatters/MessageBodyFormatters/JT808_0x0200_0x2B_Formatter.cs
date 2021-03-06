﻿using JT808.Protocol.Extensions;
using JT808.Protocol.MessageBody;
using JT808.Protocol.Interfaces;
using System;
using JT808.Protocol.MessagePack;

namespace JT808.Protocol.Formatters.MessageBodyFormatters
{
    public class JT808_0x0200_0x2B_Formatter : IJT808MessagePackFormatter<JT808_0x0200_0x2B>
    {
        public JT808_0x0200_0x2B Deserialize(ref JT808MessagePackReader reader, IJT808Config config)
        {
            JT808_0x0200_0x2B jT808LocationAttachImpl0x2B = new JT808_0x0200_0x2B();
            jT808LocationAttachImpl0x2B.AttachInfoId = reader.ReadByte();
            jT808LocationAttachImpl0x2B.AttachInfoLength = reader.ReadByte();
            jT808LocationAttachImpl0x2B.Analog = reader.ReadInt32();
            return jT808LocationAttachImpl0x2B;
        }

        public void Serialize(ref JT808MessagePackWriter writer, JT808_0x0200_0x2B value, IJT808Config config)
        {
            writer.WriteByte(value.AttachInfoId);
            writer.WriteByte(value.AttachInfoLength);
            writer.WriteInt32(value.Analog);
        }
    }
}

