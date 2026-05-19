// Assets/_Project/1_CoreDomain/Hardware/GalleryPickResult.cs
namespace Billiards.CoreDomain.Hardware
{
    public enum GalleryPickStatus
    {
        /// <summary>The pick succeeded and <see cref="GalleryPickResult.Bytes"/> is populated.</summary>
        Loaded,
        /// <summary>User cancelled the picker or denied gallery permission.</summary>
        Cancelled,
        /// <summary>Source file exceeded the configured maximum size.</summary>
        TooLarge,
        /// <summary>File path was valid but the decode step failed (corrupt image, unsupported format, etc.).</summary>
        LoadFailed,
    }

    /// <summary>
    /// Outcome of a gallery pick. Pure data envelope — callers branch on
    /// <see cref="Status"/> to decide how to render feedback to the user.
    /// </summary>
    public readonly struct GalleryPickResult
    {
        public readonly GalleryPickStatus Status;
        public readonly byte[] Bytes;
        public readonly long FileSizeBytes;

        private GalleryPickResult(GalleryPickStatus status, byte[] bytes, long fileSizeBytes)
        {
            Status = status;
            Bytes = bytes;
            FileSizeBytes = fileSizeBytes;
        }

        public static GalleryPickResult Loaded(byte[] bytes, long fileSizeBytes)
            => new GalleryPickResult(GalleryPickStatus.Loaded, bytes, fileSizeBytes);

        public static GalleryPickResult Cancelled()
            => new GalleryPickResult(GalleryPickStatus.Cancelled, null, 0);

        public static GalleryPickResult TooLarge(long fileSizeBytes)
            => new GalleryPickResult(GalleryPickStatus.TooLarge, null, fileSizeBytes);

        public static GalleryPickResult LoadFailed()
            => new GalleryPickResult(GalleryPickStatus.LoadFailed, null, 0);
    }
}