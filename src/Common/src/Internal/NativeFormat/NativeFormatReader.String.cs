// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// ---------------------------------------------------------------------------
// Native Format Reader
//
// UTF8 string reading methods
// ---------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Internal.NativeFormat
{
    internal class X3
    {
        [DllImport("*")]
        internal static unsafe extern int printf(byte* str, byte* unused);
        private static unsafe void PrintString(string s)
        {
            int length = s.Length;
            fixed (char* curChar = s)
            {
                for (int i = 0; i < length; i++)
                {
                    TwoByteStr curCharStr = new TwoByteStr();
                    curCharStr.first = (byte)(*(curChar + i));
                    printf((byte*)&curCharStr, null);
                }
            }
        }

        internal static void PrintLine(string s)
        {
            PrintString(s);
            PrintString("\n");
        }

        //public unsafe static void PrintLong(long l)
        //{
        //    PrintByte((byte)((l >> 56) & 0xff));
        //    PrintByte((byte)((l >> 48) & 0xff));
        //    PrintByte((byte)((l >> 40) & 0xff));
        //    PrintByte((byte)((l >> 32) & 0xff));
        //    PrintByte((byte)((l >> 24) & 0xff));
        //    PrintByte((byte)((l >> 16) & 0xff));
        //    PrintByte((byte)((l >> 8) & 0xff));
        //    PrintByte((byte)(l & 0xff));
        //    PrintString("\n");
        //}

        public unsafe static void PrintUint(int l)
        {
            TwoByteStr curCharStr = new TwoByteStr();

            PrintByte((byte)((l >> 24) & 0xff));
            PrintByte((byte)((l >> 16) & 0xff));
            PrintByte((byte)((l >> 8) & 0xff));
            PrintByte((byte)(l & 0xff));

            curCharStr.first = 13;
            printf((byte*)&curCharStr, null);
            curCharStr.first = 10;
            printf((byte*)&curCharStr, null);
        }

        public unsafe static void PrintByte(byte b)
        {
            TwoByteStr curCharStr = new TwoByteStr();
            var nib = (b & 0xf0) >> 4;
            curCharStr.first = (byte)((nib <= 9 ? '0' : 'A') + (nib <= 9 ? nib : nib - 10));
            printf((byte*)&curCharStr, null);
            nib = (b & 0xf);
            curCharStr.first = (byte)((nib <= 9 ? '0' : 'A') + (nib <= 9 ? nib : nib - 10));
            printf((byte*)&curCharStr, null);
        }

        public struct TwoByteStr
        {
            public byte first;
            public byte second;
        }

    }


    internal partial struct NativeParser
    {
        public string GetString()
        {
            string value;
            _offset = _reader.DecodeString(_offset, out value);
            return value;
        }

        public void SkipString()
        {
            _offset = _reader.SkipString(_offset);
        }
    }
    
    internal partial class NativeReader
    {
        public string ReadString(uint offset)
        {
            string value;
            DecodeString(offset, out value);
            return value;
        }

        public unsafe uint DecodeString(uint offset, out string value)
        {
            uint numBytes;
            offset = DecodeUnsigned(offset, out numBytes);

            if (numBytes == 0)
            {
                value = String.Empty;
                return offset;
            }

            uint endOffset = offset + numBytes;
            if (endOffset < numBytes || endOffset > _size)
                ThrowBadImageFormatException();

#if NETFX_45
            byte[] bytes = new byte[numBytes];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = *(_base + offset + i);

            value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
#else
            value = Encoding.UTF8.GetString(_base + offset, (int)numBytes);
#endif

            return endOffset;
        }

        // Decode a string, but just skip it instead of returning it
        public uint SkipString(uint offset)
        {
            uint numBytes;
            offset = DecodeUnsigned(offset, out numBytes);

            if (numBytes == 0)
            {
                return offset;
            }

            uint endOffset = offset + numBytes;
            if (endOffset < numBytes || endOffset > _size)
                ThrowBadImageFormatException();

            return endOffset;
        }

        public unsafe bool StringEquals(uint offset, string value)
        {
            uint originalOffset = offset;

            uint numBytes;
            offset = DecodeUnsigned(offset, out numBytes);

            uint endOffset = offset + numBytes;
            if (endOffset < numBytes || offset > _size)
                ThrowBadImageFormatException();

            if (numBytes < value.Length)
            {

                for (int i = 0; i < numBytes; i++)
                {

                    int ch = *(_base + offset + i);

                }
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                int ch = *(_base + offset + i);
                if (ch > 0x7F)
                    return ReadString(originalOffset) == value;

                // We are assuming here that valid UTF8 encoded byte > 0x7F cannot map to a character with code point <= 0x7F
                if (ch != value[i])
                    return false;
            }

            return numBytes == value.Length; // All char ANSI, all matching
        }
    }
}
