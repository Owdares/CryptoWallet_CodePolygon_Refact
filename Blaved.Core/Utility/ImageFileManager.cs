namespace Blaved.Core.Utility
{
    public class ImageFileManager
    {
        private static readonly Dictionary<string, SemaphoreSlim> fileSemaphores = new Dictionary<string, SemaphoreSlim>();

        public static async Task<byte[]> ReadBytesAsync(string filePath)
        {
            await GetSemaphore(filePath).WaitAsync();

            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    byte[] bytes = new byte[fileStream.Length];
                    await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length);
                    return bytes;
                }
            }
            finally
            {
                GetSemaphore(filePath).Release();
            }
        }

        private static SemaphoreSlim GetSemaphore(string filePath)
        {
            lock (fileSemaphores)
            {
                if (!fileSemaphores.TryGetValue(filePath, out var semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    fileSemaphores[filePath] = semaphore;
                }

                return semaphore;
            }
        }
    }
}
