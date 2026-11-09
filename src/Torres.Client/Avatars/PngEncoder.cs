using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Torres.Client.Avatars
{
    internal static class PngEncoder
    {
        internal const int SourceBytesPerPixel = 4;

        private const byte BitDepth = 8;
        private const byte TrueColorType = 2;
        private const byte TrueColorAlphaType = 6;
        private const byte NoFilter = 0;
        private const int HeaderDataBytes = 13;
        private const int HeaderWidthOffset = 0;
        private const int HeaderHeightOffset = 4;
        private const int HeaderBitDepthOffset = 8;
        private const int HeaderColorTypeOffset = 9;
        private const int RgbBytesPerPixel = 3;
        private const int BytesPerInteger = 4;
        private const int BitsPerByte = 8;
        private const uint LowByteMask = 0xFF;
        private const uint LowBitMask = 1;
        private const int SingleBitShift = 1;
        private const uint CrcPolynomial = 0xEDB88320;
        private const uint CrcSeed = 0xFFFFFFFF;
        private const int CrcTableSize = 256;
        private const string HeaderChunkType = "IHDR";
        private const string DataChunkType = "IDAT";
        private const string EndChunkType = "IEND";

        private static readonly byte[] _signature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly uint[] _crcTable = CreateCrcTable();

        internal static byte[] Encode(byte[] pixels, int imageSide, bool hasAlpha)
        {
            using var pngStream = new MemoryStream();
            pngStream.Write(_signature);
            WriteChunk(pngStream, HeaderChunkType, CreateHeaderData(imageSide, hasAlpha));
            WriteChunk(pngStream, DataChunkType, CompressScanlines(pixels, imageSide, hasAlpha));
            WriteChunk(pngStream, EndChunkType, Array.Empty<byte>());
            byte[] pngBytes = pngStream.ToArray();

            return pngBytes;
        }

        private static byte[] CreateHeaderData(int imageSide, bool hasAlpha)
        {
            var headerData = new byte[HeaderDataBytes];
            WriteBigEndian(headerData, HeaderWidthOffset, (uint)imageSide);
            WriteBigEndian(headerData, HeaderHeightOffset, (uint)imageSide);
            headerData[HeaderBitDepthOffset] = BitDepth;
            headerData[HeaderColorTypeOffset] = hasAlpha ? TrueColorAlphaType : TrueColorType;

            return headerData;
        }

        private static byte[] CompressScanlines(byte[] pixels, int imageSide, bool hasAlpha)
        {
            int writtenBytesPerPixel = hasAlpha ? SourceBytesPerPixel : RgbBytesPerPixel;
            using var compressedStream = new MemoryStream();
            using (var zlibStream = new ZLibStream(compressedStream, CompressionLevel.SmallestSize, true))
            {
                for (var pixelIndex = 0; pixelIndex < imageSide * imageSide; pixelIndex++)
                {
                    if (pixelIndex % imageSide == 0)
                    {
                        zlibStream.WriteByte(NoFilter);
                    }

                    zlibStream.Write(pixels, pixelIndex * SourceBytesPerPixel, writtenBytesPerPixel);
                }
            }

            byte[] compressedScanlines = compressedStream.ToArray();

            return compressedScanlines;
        }

        private static void WriteChunk(Stream pngStream, string chunkType, byte[] chunkData)
        {
            byte[] typeBytes = Encoding.ASCII.GetBytes(chunkType);
            var lengthBytes = new byte[BytesPerInteger];
            WriteBigEndian(lengthBytes, 0, (uint)chunkData.Length);
            uint chunkCrc = UpdateCrc(UpdateCrc(CrcSeed, typeBytes), chunkData) ^ CrcSeed;
            var crcBytes = new byte[BytesPerInteger];
            WriteBigEndian(crcBytes, 0, chunkCrc);
            pngStream.Write(lengthBytes);
            pngStream.Write(typeBytes);
            pngStream.Write(chunkData);
            pngStream.Write(crcBytes);
        }

        private static void WriteBigEndian(byte[] targetBytes, int targetOffset, uint integerValue)
        {
            for (var byteIndex = 0; byteIndex < BytesPerInteger; byteIndex++)
            {
                int shiftBits = (BytesPerInteger - 1 - byteIndex) * BitsPerByte;
                targetBytes[targetOffset + byteIndex] = (byte)(integerValue >> shiftBits);
            }
        }

        private static uint UpdateCrc(uint runningCrc, byte[] dataBytes)
        {
            uint updatedCrc = runningCrc;
            foreach (byte dataByte in dataBytes)
            {
                updatedCrc = _crcTable[(updatedCrc ^ dataByte) & LowByteMask] ^ (updatedCrc >> BitsPerByte);
            }

            return updatedCrc;
        }

        private static uint[] CreateCrcTable()
        {
            var crcTable = new uint[CrcTableSize];
            for (var tableIndex = 0; tableIndex < CrcTableSize; tableIndex++)
            {
                crcTable[tableIndex] = CalculateCrcEntry((uint)tableIndex);
            }

            return crcTable;
        }

        private static uint CalculateCrcEntry(uint tableIndex)
        {
            uint crcEntry = tableIndex;
            for (var bitIndex = 0; bitIndex < BitsPerByte; bitIndex++)
            {
                bool isLowBitSet = (crcEntry & LowBitMask) == LowBitMask;
                crcEntry = isLowBitSet ? (crcEntry >> SingleBitShift) ^ CrcPolynomial : crcEntry >> SingleBitShift;
            }

            return crcEntry;
        }
    }
}
