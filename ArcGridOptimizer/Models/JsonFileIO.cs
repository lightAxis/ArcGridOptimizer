using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArcGridOptimizer.ViewModels.Items;
using Google.OrTools.ConstraintSolver;

namespace ArcGridOptimizer.Models
{
    static public class JsonFileIO
    {

        private static readonly JsonSerializerOptions _opt = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void CheckAndCreateDataDirs()
        {
            CheckAndCreateDir(GetGemListDefaultPath());
            CheckAndCreateDir(GetCoreListDefaultPath());
            CheckAndCreateDir(getOptResultDefaultPath());
        }

        private static void CheckAndCreateDir(string dirpath)
        {
            if (!Directory.Exists(dirpath))
            {
                Directory.CreateDirectory(dirpath);
            }
        }
        public static string GetBinaryBasePath()
        {
            return AppContext.BaseDirectory;
        }
        public static string GetGemListDefaultPath()
        {
            return Path.Combine(GetBinaryBasePath(), Params.AppParam.DataFileDirGem);
        }
        public static string GetCoreListDefaultPath()
        {
            return Path.Combine(GetBinaryBasePath(), Params.AppParam.DataFileDirCore);
        }
        public static string getOptResultDefaultPath()
        {
            return Path.Combine(GetBinaryBasePath(), Params.AppParam.DataFileDirResult);
        }

        private static async Task SaveAsync<T>(string path, T dto, CancellationToken ct = default)
        {
            using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, dto, _opt, ct);
        }

        private static async Task<T?> LoadAsync<T>(string path, CancellationToken ct = default)
        {
            using var fs = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(fs, _opt, ct);
        }


        public static async Task<bool> SaveGemListAsync(string path, GemListDTO dto, CancellationToken ct = default)
        {
            var task = SaveAsync(path, dto, ct);
            await task.WaitAsync(TimeSpan.FromSeconds(5));
            if (!task.IsCompletedSuccessfully) return false;
            return true;
        }
        public static async Task<GemListDTO?> LoadGemListAsync(string path, CancellationToken ct = default)
        {
            var task = LoadAsync<GemListDTO>(path, ct);
            var dto = await task.WaitAsync(TimeSpan.FromSeconds(5));
            return dto;
        }

        public static async Task<bool> SaveOptResultAsync(string path, OptResultDTO dto, CancellationToken ct = default)
        {
            var task = SaveAsync<OptResultDTO>(path, dto, ct);
            await task.WaitAsync(TimeSpan.FromSeconds(5));
            if (!task.IsCompletedSuccessfully) return false;
            return true;
        }
        public static async Task<OptResultDTO?> LoadOptResultAsync(string path, CancellationToken ct = default)
        {
            var task = LoadAsync<OptResultDTO>(path, ct);
            var dto = await task.WaitAsync(TimeSpan.FromSeconds(5));
            return dto;
        }

        public static async Task<bool> SaveCoreListAsync(string path, CoreListDTO dto, CancellationToken ct = default)
        {
            var task = SaveAsync<CoreListDTO>(path, dto, ct);
            await task.WaitAsync(TimeSpan.FromSeconds(5));
            if (!task.IsCompletedSuccessfully) return false;
            return true;
        }
        public static async Task<CoreListDTO?> LoadCoreListAsync(string path, CancellationToken ct = default)
        {
            var task = LoadAsync<CoreListDTO>(path, ct);
            var dto = await task.WaitAsync(TimeSpan.FromSeconds(5));
            return dto;
        }

    }
}
