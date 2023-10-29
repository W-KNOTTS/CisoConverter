using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using K4os.Compression.LZ4;

namespace CisoConverter
{
    // Declare the CsoCompressionStream class, inheriting from Stream
    public sealed class CsoCompressionStream : Stream
    {
        // Various constants related to the CSO file format
        public const int CisoHeaderSize = 0x18;
        public const uint CisoCompressionMarker = 0x80000000;
        public const uint FileSplitBoundary = 0xFFBF6000;

        // Member variables for internal operations
        private readonly BinaryWriter _writeBase;
        private readonly int _blockSize;
        private long _totalBytes;
        private bool _writtenHeader;
        private int _numBlocks;
        private uint[] _blockIndex;
        private readonly byte[] _compressionBuffer;
        private readonly byte[] _currentBlockData;
        private int _currentBlock = 0;
        private int _offsetInBlock = 0;
        private int _currentBlockSize;
        internal Action<double> ProgressChanged;
        private readonly int _version = 2;
        private readonly int _align = 2;
        private readonly int _alignBytes;
        private readonly LZ4Level _compressionLevel;
        String myCSO = "";
        String FullPath = "";
        long maxSize = 4285913201L;

        public void myCsoPath(string mycso, string csoPATH)
        {
            FullPath = csoPATH;
            myCSO = mycso;
        }
        // Constructor for the class
        public CsoCompressionStream(Stream baseStream, long totalBytes, LZ4Level compressionLevel = LZ4Level.L12_MAX, int blockSize = 2048)
        {
            // Initialize progress
            double progress = (double)(_currentBlock * _blockSize) / _totalBytes * 100;
            Console.WriteLine($"Progress: {progress}");
            ProgressChanged?.Invoke(progress);

            // Stream validations
            if (!baseStream.CanSeek || !baseStream.CanWrite)
                throw new ArgumentException("supplied stream must support seeking and writing - write to a temporary file or memory stream before writing to a network pipe");

            // Initialize variables
            _writeBase = new BinaryWriter(baseStream);
            _blockSize = blockSize;
            _compressionBuffer = new byte[_blockSize * 2];
            _currentBlockData = new byte[_blockSize];
            _alignBytes = 1 << _align;
            _compressionLevel = compressionLevel;
            SetLength(totalBytes);
        }

        // Write CSO header
        private void WriteCsoHeader()
        {
            _writtenHeader = true;
            _writeBase.Write(Encoding.ASCII.GetBytes("CISO"));
            _writeBase.Write(CisoHeaderSize);
            _writeBase.Write(_totalBytes);
            _writeBase.Write(_blockSize);
            _writeBase.Write((byte)_version);
            _writeBase.Write((byte)_align);
            _writeBase.Write((short)0);
        }

        // Write the block index into the file
        private void WriteBlockIndex()
        {
            _writeBase.Seek(CisoHeaderSize, SeekOrigin.Begin);
            foreach (var b in _blockIndex)
            {
                _writeBase.Write(b);
            }
        }
        // Added function to split the .cso file into multiple files
        public void SplitCsoFile(string csoPath)
        {
            
            // Open the source file for reading
            using (var sourceStream = new FileStream(csoPath, FileMode.Open, FileAccess.Read))
            {
                int partNumber = 1;
                long bytesRemaining = sourceStream.Length;
                byte[] buffer = new byte[1024 * 1024]; // 1 MB buffer

                while (bytesRemaining > 0)
                {
                    string newFileName = $"{csoPath}.{partNumber}.cso";
                    using (var destinationStream = new FileStream(newFileName, FileMode.Create, FileAccess.Write))
                    {
                        long writtenBytes = 0;
                        while (writtenBytes < maxSize)
                        {
                            int bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                            {
                                break;
                            }

                            destinationStream.Write(buffer, 0, bytesRead);
                            bytesRemaining -= bytesRead;
                            writtenBytes += bytesRead;
                        }
                    }

                    partNumber++;
                }
            }
        }



        // Write to the compression stream
        public override void Write(byte[] buffer, int offset, int count)
        {
            // Stream length check
            if ((_currentBlock - 1) * _blockSize + _offsetInBlock + count > _totalBytes)
                throw new EndOfStreamException("attempted to write more data than initially specified");

            // Main write loop
            while (count > 0)
            {
                var toCopy = Math.Min(count, _currentBlockSize - _offsetInBlock);
                Array.Copy(buffer, offset, _currentBlockData, _offsetInBlock, toCopy);
                count -= toCopy;
                offset += toCopy;
                _offsetInBlock += toCopy;

                if (_offsetInBlock != _currentBlockSize) continue;

                // Compression logic
                var compressedSize = LZ4Codec.Encode(_currentBlockData, 0, _currentBlockSize, _compressionBuffer, 0,
                    _compressionBuffer.Length, _compressionLevel);

                // Write header if necessary
                if (!_writtenHeader)
                {
                    WriteCsoHeader();
                    WriteBlockIndex();
                }

                // Alignment logic
                var alignBytes = (_alignBytes - (_writeBase.BaseStream.Position % _alignBytes)) % _alignBytes;
                for (var i = 0; i < alignBytes; i++)
                {
                    _writeBase.Write((byte)0);
                }

                // Block index logic
                _blockIndex[_currentBlock] = (uint)((Position > FileSplitBoundary ? Position - FileSplitBoundary : Position) >> _align);

                // Block writing logic
                if (compressedSize + 12 >= _currentBlockSize)
                {
                    _writeBase.Write(_currentBlockData, 0, _currentBlockSize);
                }
                else
                {
                    _blockIndex[_currentBlock] |= CisoCompressionMarker;
                    _writeBase.Write(compressedSize);
                    _writeBase.Write(_compressionBuffer, 0, compressedSize);
                    _writeBase.Write(0);
                }

                _currentBlock += 1;
                _currentBlockSize = _blockSize;
                _offsetInBlock = 0;

                // Progress update
                double progress = (double)(_currentBlock * _blockSize) / _totalBytes * 100;
                ProgressChanged?.Invoke(progress);

                // Last block logic
                if (_currentBlock == _numBlocks - 1)
                {
                    _currentBlockSize = (int)(_totalBytes - ((long)_blockSize * _currentBlock));
                }
                else if (_currentBlock == _numBlocks)
                {
                    _blockIndex[_numBlocks] = (uint)((Position > FileSplitBoundary ? Position - FileSplitBoundary : Position) >> _align);
                    WriteBlockIndex();
                }
            }
        }
       
        public void splitCSO()
        {
            //if ($@"{myCSO}.cso" >= maxSize)
            FileInfo fileInfo = new FileInfo($@"{FullPath}.cso");
            long fileSize = fileInfo.Length;

            // Check if the file size exceeds the maximum allowed size
            if (fileSize >= maxSize)
            {
                // Perform the split
                SplitCsoFile($@"{FullPath}.cso");
            }
        }

        // Flush the writer
        public override void Flush()
        {
            _writeBase.Flush();
        }

        // Reading not supported
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Direct reading is not supported.");
        }

        // Seeking not supported
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seeking is not supported.");
        }

        // Set the length of the stream
        public override void SetLength(long value)
        {
            if (_writtenHeader) throw new IOException("Length must be set before initial write");

            _totalBytes = value;
            _currentBlockSize = (int)Math.Min(_blockSize, _totalBytes);
            _numBlocks = (int)(_totalBytes / _blockSize);
            _blockIndex = new uint[_numBlocks + 1];
        }

        // Stream capabilities
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _totalBytes;
        public override long Position
        {
            get => _writeBase.BaseStream.Position;
            set => throw new NotSupportedException("Setting the position is not supported.");
        }

        // Dispose the writer
        private new void Dispose(bool disposing)
        {
            _writeBase.Dispose();
        }
    }
}
