﻿using JT808.Protocol.Buffers;
using JT808.Protocol.Extensions;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;


namespace JT808.Protocol.MessagePack
{
    public ref struct JT808MessagePackReader
    {
        public ReadOnlySpan<byte> Reader { get; private set; }
        public ReadOnlySpan<byte> SrcBuffer { get; }
        public int ReaderCount { get; private set; }
        private byte _calculateCheckXorCode;
        private byte _realCheckXorCode;
        private bool _checkXorCodeVali;
        /// <summary>
        /// 是否进行解码操作
        /// 若进行解码操作，则对应的是一个正常的包
        /// 若不进行解码操作，则对应的是一个非正常的包（头部包，数据体包等等）
        /// 主要用来一次性读取所有数据体内容操作
        /// </summary>
        private bool _decoded;
        private static byte[] decode7d01 = new byte[] { 0x7d, 0x01 };
        private static byte[] decode7d02 = new byte[] { 0x7d, 0x02 };
        /// <summary>
        /// 解码（转义还原）,计算校验和
        /// </summary>
        /// <param name="buffer"></param>
        public JT808MessagePackReader(ReadOnlySpan<byte> srcBuffer)
        {
            SrcBuffer = srcBuffer;
            ReaderCount = 0;
            _realCheckXorCode = 0x00;
            _calculateCheckXorCode = 0x00;
            _checkXorCodeVali = false;
            _decoded = false;
            Reader = srcBuffer;
        }
        /// <summary>
        /// 在解码的时候把校验和也计算出来，避免在循环一次进行校验
        /// </summary>
        /// <returns></returns>
        public void Decode()
        {
            Span<byte> span = new byte[SrcBuffer.Length];
            Decode(span);
            _decoded = true;
        }
        /// <summary>
        /// 在解码的时候把校验和也计算出来，避免在循环一次进行校验
        /// </summary>
        /// <returns></returns>
        public void Decode(Span<byte> allocateBuffer)
        {
            int i = 0;
            int offset = 0;
            int len = SrcBuffer.Length;
            _realCheckXorCode = 0;
            allocateBuffer[offset++] = SrcBuffer[0];
            // 取出校验码看是否需要转义
            ReadOnlySpan<byte> checkCodeBufferSpan = SrcBuffer.Slice(len - 3,2);
            int checkCodeLen = 1;
            if (checkCodeBufferSpan.SequenceEqual(decode7d01))
            {
                _realCheckXorCode = 0x7d;
                checkCodeLen += 1;
            }
            else if (checkCodeBufferSpan.SequenceEqual(decode7d02))
            {
                _realCheckXorCode = 0x7e;
                checkCodeLen += 1;
            }
            else
            {
                _realCheckXorCode = checkCodeBufferSpan[1];
            }
            len = len - checkCodeLen - 1 - 1;
            ReadOnlySpan<byte> tmpBufferSpan = SrcBuffer.Slice(1, len);
            while (i < len)
            {
                if (tmpBufferSpan[i] == 0x7d)
                {
                    if (len > i + 1)
                    {
                        if (tmpBufferSpan[i + 1] == 0x01)
                        {
                            allocateBuffer[offset++] = 0x7d;
                            _calculateCheckXorCode = (byte)(_calculateCheckXorCode ^ 0x7d);
                            i++;
                        }
                        else if (tmpBufferSpan[i + 1] == 0x02)
                        {
                            allocateBuffer[offset++] = 0x7e;
                            _calculateCheckXorCode = (byte)(_calculateCheckXorCode ^ 0x7e);
                            i++;
                        }
                        else
                        {
                            allocateBuffer[offset++] = tmpBufferSpan[i];
                            _calculateCheckXorCode = (byte)(_calculateCheckXorCode ^ tmpBufferSpan[i]);
                        }
                    }
                }
                else
                {
                    allocateBuffer[offset++] = tmpBufferSpan[i];
                    _calculateCheckXorCode = (byte)(_calculateCheckXorCode ^ tmpBufferSpan[i]);
                }
                i++;
            }
            allocateBuffer[offset++] = _realCheckXorCode;
            allocateBuffer[offset++] = SrcBuffer[SrcBuffer.Length- 1];
            _checkXorCodeVali = (_calculateCheckXorCode == _realCheckXorCode);
            Reader = allocateBuffer.Slice(0, offset);
            _decoded = true;
        }
        public byte CalculateCheckXorCode => _calculateCheckXorCode;
        public byte RealCheckXorCode => _realCheckXorCode;
        public bool CheckXorCodeVali => _checkXorCodeVali;
        public byte ReadStart()=> ReadByte();
        public byte ReadEnd()=> ReadByte();
        public ushort ReadUInt16()
        {
            var readOnlySpan = GetReadOnlySpan(2);
            ushort value = (ushort)((readOnlySpan[0] << 8) | (readOnlySpan[1]));
            return value;
        }
        public uint ReadUInt32()
        {
            var readOnlySpan = GetReadOnlySpan(4);
            uint value = (uint)((readOnlySpan[0] << 24) | (readOnlySpan[1] << 16) | (readOnlySpan[2] << 8) | readOnlySpan[3]);
            return value;
        }
        public int ReadInt32()
        {
            var readOnlySpan = GetReadOnlySpan(4);
            int value = (int)((readOnlySpan[0] << 24) | (readOnlySpan[1] << 16) | (readOnlySpan[2] << 8) | readOnlySpan[3]);
            return value;
        }
        public ulong ReadUInt64()
        {
            var readOnlySpan = GetReadOnlySpan(8);
            ulong value = (ulong)(
                (readOnlySpan[0] << 56) |
                (readOnlySpan[1] << 48) |
                (readOnlySpan[2] << 40) |
                (readOnlySpan[3] << 32) |
                (readOnlySpan[4] << 24) |
                (readOnlySpan[5] << 16) |
                (readOnlySpan[6] << 8) |
                 readOnlySpan[7]);
            return value;
        }
        public byte ReadByte()
        {
            var readOnlySpan = GetReadOnlySpan(1);
            return readOnlySpan[0];
        }
        public byte ReadVirtualByte()
        {
            var readOnlySpan = GetVirtualReadOnlySpan(1);
            return readOnlySpan[0];
        }
        public ushort ReadVirtualUInt16()
        {
            var readOnlySpan = GetVirtualReadOnlySpan(2);
            return (ushort)((readOnlySpan[0] << 8) | (readOnlySpan[1]));
        }
        public uint ReadVirtualUInt32()
        {
            var readOnlySpan = GetVirtualReadOnlySpan(4);
            return (uint)((readOnlySpan[0] << 24) | (readOnlySpan[1] << 16) | (readOnlySpan[2] << 8) | readOnlySpan[3]);
        }
        public ulong ReadVirtualUInt64()
        {
            var readOnlySpan = GetVirtualReadOnlySpan(8);
            return (ulong)(
                (readOnlySpan[0] << 56) |
                (readOnlySpan[1] << 48) |
                (readOnlySpan[2] << 40) |
                (readOnlySpan[3] << 32) |
                (readOnlySpan[4] << 24) |
                (readOnlySpan[5] << 16) |
                (readOnlySpan[6] << 8) |
                 readOnlySpan[7]);
        }

        /// <summary>
        /// 数字编码 大端模式、高位在前
        /// </summary>
        /// <param name="len"></param>
        public string ReadBigNumber(int len)
        {
            ulong result = 0;
            var readOnlySpan = GetReadOnlySpan(len);
            for (int i = 0; i < len; i++)
            {
                ulong currentData = (ulong)readOnlySpan[i] << (8 * (len - i - 1));
                result += currentData;
            }
            return result.ToString();
        }
        public ReadOnlySpan<byte> ReadArray(int len)
        {
            var readOnlySpan = GetReadOnlySpan(len);
            return readOnlySpan.Slice(0, len);
        }
        public ReadOnlySpan<byte> ReadArray(int start,int end)
        {
            return Reader.Slice(start,end);
        }
        public string ReadString(int len)
        {
            var readOnlySpan = GetReadOnlySpan(len);
            string value = JT808Constants.Encoding.GetString(readOnlySpan.Slice(0, len).ToArray());
            return value.Trim('\0');
        }
        public string ReadRemainStringContent()
        {
            var readOnlySpan = ReadContent(0);
            string value = JT808Constants.Encoding.GetString(readOnlySpan.ToArray());
            return value.Trim('\0');
        }
        public string ReadHex(int len)
        {
            var readOnlySpan = GetReadOnlySpan(len);
            string hex = HexUtil.DoHexDump(readOnlySpan, 0, len);
            return hex;
        }
        /// <summary>
        /// yyMMddHHmmss
        /// </summary>
        /// <param name="fromBase">>D2： 10  X2：16</param>
        public DateTime ReadDateTime6(string format = "X2")
        {
            DateTime d;
            try
            {
                var readOnlySpan = GetReadOnlySpan(6);
                int year = Convert.ToInt32(readOnlySpan[0].ToString(format)) + JT808Constants.DateLimitYear;
                int month = Convert.ToInt32(readOnlySpan[1].ToString(format));
                int day = Convert.ToInt32(readOnlySpan[2].ToString(format));
                int hour = Convert.ToInt32(readOnlySpan[3].ToString(format));
                int minute = Convert.ToInt32(readOnlySpan[4].ToString(format));
                int second = Convert.ToInt32(readOnlySpan[5].ToString(format));
                d = new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception)
            {
                d = JT808Constants.UTCBaseTime;
            }
            return d;
        }
        /// <summary>
        /// HH-mm-ss-msms
        /// HH-mm-ss-fff
        /// </summary>
        /// <param name="format">D2： 10  X2：16</param>
        public DateTime ReadDateTime5(string format = "X2")
        {
            DateTime d;
            try
            {
                var readOnlySpan = GetReadOnlySpan(5);
                d = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                Convert.ToInt32(readOnlySpan[0].ToString(format)),
                Convert.ToInt32(readOnlySpan[1].ToString(format)),
                Convert.ToInt32(readOnlySpan[2].ToString(format)),
                Convert.ToInt32(((readOnlySpan[3] << 8) + readOnlySpan[4])));
            }
            catch
            {
                d = JT808Constants.UTCBaseTime;
            }
            return d;
        }
        /// <summary>
        /// YYYYMMDD
        /// </summary>
        /// <param name="format">D2： 10  X2：16</param>
        public DateTime ReadDateTime4(string format = "X2")
        {
            DateTime d;
            try
            {
                var readOnlySpan = GetReadOnlySpan(4);
                d = new DateTime(
               (Convert.ToInt32(readOnlySpan[0].ToString(format)) << 8) + Convert.ToByte(readOnlySpan[1]),
                Convert.ToInt32(readOnlySpan[2].ToString(format)),
                Convert.ToInt32(readOnlySpan[3].ToString(format)));
            }
            catch (Exception)
            {
                d = JT808Constants.UTCBaseTime;
            }
            return d;   
        }
        public DateTime ReadUTCDateTime()
        {
            DateTime d;
            try
            {
                ulong result = 0;
                var readOnlySpan = GetReadOnlySpan(8);
                for (int i = 0; i < 8; i++)
                {
                    ulong currentData = (ulong)readOnlySpan[i] << (8 * (8 - i - 1));
                    result += currentData;
                }
                d = JT808Constants.UTCBaseTime.AddSeconds(result).AddHours(8);
            }
            catch (Exception)
            {
                d = JT808Constants.UTCBaseTime;
            }
            return d;
        }
        public string ReadBCD(int len)
        {
            int count = len / 2;
            var readOnlySpan = GetReadOnlySpan(count);
            StringBuilder bcdSb = new StringBuilder(count);
            for (int i = 0; i < count; i++)
            {
                bcdSb.Append(readOnlySpan[i].ToString("X2"));
            }
            // todo:对于协议来说这个0是有意义的，下个版本在去掉
            return bcdSb.ToString().TrimStart('0');
        }
        private ReadOnlySpan<byte> GetReadOnlySpan(int count)
        {
            ReaderCount += count;
            return Reader.Slice(ReaderCount - count);
        }
        public ReadOnlySpan<byte> GetVirtualReadOnlySpan(int count)
        {
            return Reader.Slice(ReaderCount, count);
        }
        public ReadOnlySpan<byte> ReadContent(int count=0)
        {
            if (_decoded)
            {
                //内容长度=总长度-读取的长度-2（校验码1位+终止符1位）
                int totalContent = Reader.Length - ReaderCount - 2;
                //实际读取内容长度
                int realContent = totalContent - count;
                int tempReaderCount = ReaderCount;
                ReaderCount += realContent;
                return Reader.Slice(tempReaderCount, realContent);
            }
            else
            {
                return Reader.Slice(ReaderCount);
            }
        }
        public int ReadCurrentRemainContentLength()
        {
            if (_decoded)
            {
                //内容长度=总长度-读取的长度-2（校验码1位+终止符1位）
                return Reader.Length - ReaderCount - 2; 
            }
            else
            {
                return Reader.Length - ReaderCount;
            }
        }
        public void Skip(int count=1)
        {
            ReaderCount += count;
        }
    }
}
