namespace SpyroETDChunkTool
{
    public class FileEntry
    {
        public uint Hash;
        public uint Offset;
        public uint Size;
        public uint Unknown;

        public byte[] Data;
    }
}
