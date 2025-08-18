// ================================
// FILE: Assets/Editor/MugenSffV2Parser.cs
// DESCRIPTION: Minimal SFF v2 reader for M.U.G.E.N 1.1 that extracts subfiles containing PNG/JPG payloads.
// NOTE: This does not handle SFF v1 PCX. Extend if needed.
// ================================
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MugenSffV2Parser
{
    public class Subfile
    {
        public int GroupNumber;
        public int ImageNumber;
        public int Width;
        public int Height;
        public int XAxis;
        public int YAxis;
        public byte[] ImageBytes; // decoded PNG/JPG payload
    }

    private readonly byte[] _bytes;
    public Dictionary<int, Subfile> Subfiles = new Dictionary<int, Subfile>(); // key: index

    public MugenSffV2Parser(byte[] bytes) { _bytes = bytes; }

    public void Parse()
    {
        using (var ms = new MemoryStream(_bytes))
        using (var br = new BinaryReader(ms))
        {
            // Header v2 signature: "ElecbyteSpr"
            var sig = new string(br.ReadChars(12));
            if (!sig.StartsWith("ElecbyteSpr", StringComparison.Ordinal))
                throw new Exception("Not an SFF v2 file (missing ElecbyteSpr header).");

            // v2 header layout (simplified):
            // 0x00: signature (12)
            // 0x0C: version (4 bytes)
            // 0x10: reserved (8)
            // 0x18: number of groups (4)
            // 0x1C: number of images (4)
            // 0x20: subfile header offset (4)
            // 0x24: subfile header length (4)
            // 0x28: palette type (4) ... (we ignore for PNG/JPG)

            uint version = br.ReadUInt32();
            br.ReadBytes(8); // reserved
            uint numGroups = br.ReadUInt32();
            uint numImages = br.ReadUInt32();
            uint subHeaderOffset = br.ReadUInt32();
            uint subHeaderLength = br.ReadUInt32();
            br.ReadUInt32(); // palette type

            // Jump to subfile headers
            ms.Seek(subHeaderOffset, SeekOrigin.Begin);

            for (int i = 0; i < numImages; i++)
            {
                long entryPos = ms.Position;
                // Subfile header (simplified per Elecbyte docs / common implementations)
                uint nextOffset = br.ReadUInt32();          // offset of next subfile header
                uint fileOffset = br.ReadUInt32();          // offset of raw image data
                uint fileLength = br.ReadUInt32();          // length of raw image data
                uint axisX = br.ReadUInt32();               // axis X
                uint axisY = br.ReadUInt32();               // axis Y
                uint groupNo = br.ReadUInt32();             // group number
                uint imageNo = br.ReadUInt32();             // image number
                uint format = br.ReadUInt32();              // 0 = raw/pcx, 1 = png, 2 = jpg (common)
                uint width = br.ReadUInt32();
                uint height = br.ReadUInt32();

                var sub = new Subfile
                {
                    GroupNumber = (int)groupNo,
                    ImageNumber = (int)imageNo,
                    Width = (int)width,
                    Height = (int)height,
                    XAxis = (int)axisX,
                    YAxis = (int)axisY,
                };

                if (fileLength > 0 && (format == 1 || format == 2))
                {
                    long cur = ms.Position;
                    ms.Seek(fileOffset, SeekOrigin.Begin);
                    sub.ImageBytes = br.ReadBytes((int)fileLength);
                    ms.Seek(cur, SeekOrigin.Begin);
                }
                else
                {
                    // Non-PNG/JPG formats are not handled in this minimal parser
                    sub.ImageBytes = Array.Empty<byte>();
                }

                Subfiles[i] = sub;

                if (nextOffset == 0) break; // last
                ms.Seek(nextOffset, SeekOrigin.Begin);
            }
        }
    }
}