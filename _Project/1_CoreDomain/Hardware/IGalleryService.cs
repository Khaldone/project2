using Cysharp.Threading.Tasks;

namespace Billiards.CoreDomain.Hardware
{
    public interface IGalleryService
    {
        /// <summary>
        /// Prompts the user to pick an image from their device gallery.
        /// Compresses the image and returns it as a byte array (e.g., JPG).
        /// </summary>
        /// <param name="maxSize">The maximum width or height of the image. The image is downscaled proportionally if larger.</param>
        /// <param name="jpgQuality">The JPG compression quality (0-100).</param>
        /// <returns>The compressed image bytes, or null if the user cancelled or denied permission.</returns>
        UniTask<byte[]> PickAndCompressImageAsync(int maxSize = 512, int jpgQuality = 85);

        /// <summary>
        /// Picks an image from the gallery and returns it as lossless PNG bytes,
        /// rejecting source files that exceed <paramref name="maxFileSizeBytes"/>.
        /// The file-size check runs BEFORE decode so oversized images can't spike
        /// memory on low-RAM devices.
        ///
        /// Intended for content-moderation pipelines that need pixel fidelity
        /// comparable to a Unity-asset Texture2D (i.e. no JPG quantisation artefacts).
        /// </summary>
        /// <param name="maxSize">Maximum width or height. Downsampling is done on disk before decode.</param>
        /// <param name="maxFileSizeBytes">Hard cap on source file size in bytes. Defaults to 25 MB.</param>
        UniTask<GalleryPickResult> PickImageLosslessAsync(int maxSize = 1024, long maxFileSizeBytes = 25L * 1024 * 1024);
    }
}