using Newtonsoft.Json;

namespace Blaved.Core.Utility
{
    public class JsonFileManager
    {
        private static Dictionary<string, SemaphoreSlim> fileSemaphores = new Dictionary<string, SemaphoreSlim>();

        public static async Task PutToJsonAsync<T>(string filePath, T data) where T : class 
        {
            string json = JsonConvert.SerializeObject(data);

            await GetSemaphore(filePath).WaitAsync();

            try
            {
                await File.WriteAllTextAsync(filePath, json);
            }
            finally
            {
                GetSemaphore(filePath).Release();
            }
        }

        public static async Task<T?> GetFromJsonAsync<T>(string filePath)
        {
            await GetSemaphore(filePath).WaitAsync();

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                return JsonConvert.DeserializeObject<T>(json) ?? throw new Exception();
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
