// Import the required namespaces
using System;
using System.IO;
using System.Text;
using K4os.Compression.LZ4;

// Declare the namespace
namespace CisoConverter;

// Declare a public sealed class that inherits from Stream
public sealed class CsoDecompressionStream : Stream
{
    // Declare a private readonly BinaryReader field to read from the base stream
    private readonly BinaryReader _base;
    // Declare an internal Action to report progress changes
    internal Action<double> ProgressChanged;

    // Fields to hold compressed ISO information
    private readonly long _totalBytes;
    private readonly uint _blockSize;
    private readonly byte _align;
    private readonly uint[] _blockIndex;

    // Fields for read/write operations
    private int _currentBlock = 0;
    private int _offsetInBlock = 0;
    private readonly byte[] _decompressionBuffer;
    private readonly byte[] _currentBlockData;

    // Constructor
    public CsoDecompressionStream(Stream source)
    {
        // Initialize BinaryReader and read file headers
        _base = new BinaryReader(source);
        // Validate CISO file header
        if (!Encoding.ASCII.GetString(_base.ReadBytes(4)).Equals("CISO"))
            throw new IOException("Not a valid CISO file");
        // Read header size, total bytes, and block size
        var headerSize = _base.ReadUInt32();
        _totalBytes = _base.ReadInt64();
        _blockSize = _base.ReadUInt32();
        _decompressionBuffer = new byte[_blockSize + 4];
        // Check CISO version
        var version = _base.ReadByte();
        if (version > 2) throw new IOException($"Unsupported CISO version: {version}");
        // Read alignment byte and skip 2 bytes
        _align = _base.ReadByte();
        _base.BaseStream.Position += 2;

        // Initialize block index
        source.Position = headerSize;
        _blockIndex = new uint[(int)(_totalBytes / _blockSize) + 1];
        for (var i = 0; i < _blockIndex.Length; i++)
        {
            _blockIndex[i] = _base.ReadUInt32();
        }

        // Initialize the block data buffer
        _currentBlockData = new byte[_blockSize];
    }

    // Function to update the progress bar
    private void UpdateProgress()
    {
        // Calculate progress as a percentage and invoke the progress changed event
        double progress = (double)(_currentBlock * _blockSize) / _totalBytes * 100;
        ProgressChanged?.Invoke(progress);
    }

    // Overridden Flush method
    public override void Flush()
    {
        // Throws a NotImplementedException if called
        throw new NotImplementedException();
    }

    // Overridden Read method
    public override int Read(byte[] buffer, int offset, int count)
    {
        // Call to update the progress
        UpdateProgress();
        // Declare variables to hold read count and perform reading
        var read = 0;
        while (read < count)
        {
            // Remaining bytes to read and remaining bytes in the current block
            var remaining = count - read;
            var remainingInBlock = _blockSize - _offsetInBlock;
            // Calculate how many bytes can be read
            var canRead = (int)Math.Min(remaining, remainingInBlock);
            // Copy data from the current block to the buffer
            Array.Copy(_currentBlockData, _offsetInBlock, buffer, offset, canRead);

            // Update offsets and read counts
            _offsetInBlock += canRead;
            offset += canRead;
            read += canRead;

            // Check if we've reached the end of the current block
            if (_offsetInBlock != _blockSize) continue;
            // Check if we've reached the end of the file
            if (_currentBlock >= _blockIndex.Length - 2)
            {
                _currentBlock += 1;
                if (_currentBlock > _blockIndex.Length)
                    throw new EndOfStreamException();
                return read;
            }

            // Read the next block
            ReadBlock(_currentBlock + 1);
        }

        return read;
    }

    // Overridden Seek method
    public override long Seek(long offset, SeekOrigin origin)
    {
        // Calculate the real offset based on the SeekOrigin
        var realOffset = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => offset
        };

        // Calculate the block number and read it
        var block = (int)(realOffset / _blockSize);
        ReadBlock(block);
        // Update the offset within the block
        _offsetInBlock = (int)(realOffset % _blockSize);

        return Position;
    }

    // Private method to calculate the file offset for a block
    private long OffsetForBlock(int block)
    {
        // Calculate the offset for the block
        return (_blockIndex[block] & 0x7fffffff) << _align;
    }

    // Private method to read a block
    private void ReadBlock(int block)
    {
        // Check if the block index is valid
        if (block >= _blockIndex.Length)
            throw new EndOfStreamException();

        // Calculate the file offset and size for the block
        var blockOffset = OffsetForBlock(block);
        var size = (int)(OffsetForBlock(block + 1) - blockOffset);

        // Position the reader at the block offset
        _base.BaseStream.Position = blockOffset;

        // Check if the block is LZ4 compressed and read it accordingly
        if ((_blockIndex[block] & 0x80000000) != 0)
        {
            // LZ4 compressed block
            var outOff = 0;
            while (true)
            {
                // Read block size and check for uncompressed blocks
                var bSize = _base.ReadInt32();
                if (bSize == 0)
                {
                    break;
                }

                var uncompressed = false;
                if (bSize < 0)
                {
                    uncompressed = true;
                    bSize = -bSize;
                }

                // Read the compressed or uncompressed data into a buffer
                for (var off = 0; off < bSize;)
                {
                    off += _base.Read(_decompressionBuffer, off, bSize - off);
                }

                // Decompress the block if necessary
                if (uncompressed)
                {
                    Array.Copy(_decompressionBuffer, 0, _currentBlockData, outOff, bSize);
                    outOff += bSize;
                }
                else
                {
                    var decompressed = LZ4Codec.Decode(_decompressionBuffer, 0, bSize, _currentBlockData, outOff,
                        (int)_blockSize - outOff);
                    if (decompressed < 0)
                    {
                        throw new Exception("FML");
                    }

                    outOff += decompressed;
                }

                // Break if we've read the entire block
                if (outOff == _blockSize)
                {
                    break;
                }
            }
        }
        else
        {
            // Uncompressed block
            for (var off = 0; off < size;)
            {
                off += _base.Read(_currentBlockData, off, size - off);
            }
        }

        // Update the current block and reset the offset within the block
        _currentBlock = block;
        _offsetInBlock = 0;
    }

    // Overridden SetLength method
    public override void SetLength(long value)
    {
        // Throws a NotImplementedException if called
        throw new System.NotImplementedException();
    }

    // Overridden Write method
    public override void Write(byte[] buffer, int offset, int count)
    {
        // Throws a NotImplementedException if called
        throw new System.NotImplementedException();
    }

    // Overridden properties
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _totalBytes;

    public override long Position
    {
        // Get the current position
        get => _currentBlock * _blockSize + _offsetInBlock;
        // Set the current position
        set => Seek(value, SeekOrigin.Begin);
    }

    // Private Dispose method
    private new void Dispose(bool disposing)
    {
        // Dispose of the BinaryReader
        _base.Dispose();
    }
}
