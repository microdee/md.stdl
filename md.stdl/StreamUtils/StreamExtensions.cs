﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace md.stdl.StreamUtils
{
    public static class StreamExtensions
    {
        public static bool ReadBool(this Stream input)
        {
            byte[] tmp = new byte[1];
            input.Read(tmp, 0, 1);
            return BitConverter.ToBoolean(tmp, 0);
        }
        public static uint ReadUint(this Stream input)
        {
            byte[] tmp = new byte[4];
            input.Read(tmp, 0, 4);
            return BitConverter.ToUInt32(tmp, 0);
        }
        public static int ReadInt(this Stream input)
        {
            byte[] tmp = new byte[4];
            input.Read(tmp, 0, 1);
            return BitConverter.ToInt32(tmp, 0);
        }
        public static float ReadFloat(this Stream input)
        {
            byte[] tmp = new byte[4];
            input.Read(tmp, 0, 1);
            return BitConverter.ToSingle(tmp, 0);
        }
        public static double ReadDouble(this Stream input)
        {
            byte[] tmp = new byte[8];
            input.Read(tmp, 0, 1);
            return BitConverter.ToDouble(tmp, 0);
        }

        public static void WriteBool(this Stream input, bool data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUint(this Stream input, uint data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteInt(this Stream input, int data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteFloat(this Stream input, float data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteDouble(this Stream input, double data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }


        public static string ReadASCII(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return Encoding.ASCII.GetString(tmp);
        }
        public static string ReadUTF8(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return Encoding.UTF8.GetString(tmp);
        }
        public static string ReadUnicode(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return Encoding.Unicode.GetString(tmp);
        }

        public static void WriteASCII(this Stream input, string data)
        {
            byte[] tmp = Encoding.ASCII.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUTF8(this Stream input, string data)
        {
            byte[] tmp = Encoding.UTF8.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUnicode(this Stream input, string data)
        {
            byte[] tmp = Encoding.Unicode.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }

        public static uint ASCIILength(this string s)
        {
            byte[] tmp = Encoding.ASCII.GetBytes(s);
            return (uint)tmp.Length;
        }
        public static uint UTF8Length(this string s)
        {
            byte[] tmp = Encoding.UTF8.GetBytes(s);
            return (uint)tmp.Length;
        }
        public static uint UnicodeLength(this string s)
        {
            byte[] tmp = Encoding.Unicode.GetBytes(s);
            return (uint)tmp.Length;
        }

        public static Stream ToStream(this byte[] b)
        {
            Stream s = new MemoryStream();
            s.Read(b, 0, b.Length);
            return s;
        }
    }
}
#pragma warning restore CS1591