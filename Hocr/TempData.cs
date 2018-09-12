using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Hocr
{
    public class TempData : IDisposable
    {
        private readonly Dictionary<string, string> _caches = new Dictionary<string, string>();

        private string TemporaryFilePath { get; } = // @"C:\HocrCache\";
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");

        private TempData()
        {
            if (Directory.Exists(TemporaryFilePath))
                return;
            try
            {
                Directory.CreateDirectory(TemporaryFilePath);
            }
            catch (Exception)
            {
                throw new Exception("Cannot create Cache Folder");
            }
        }

        private static readonly Lazy<TempData> LazyInstance = new Lazy<TempData>(CreateInstanceOfT, LazyThreadSafetyMode.ExecutionAndPublication);

        public static TempData Instance => LazyInstance.Value;

        private static TempData CreateInstanceOfT() { return Activator.CreateInstance(typeof(TempData), true) as TempData; }

        public string CreateNewSession(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName))
                throw new Exception("Session name cannot be empty!");

            if (_caches.ContainsKey(sessionName))
                throw new Exception("Session already exist!");

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");

            string newFolderName = rgx.Replace(sessionName, "");

            string originalName = newFolderName;

            int counter = 0;

            while (Directory.Exists(Path.Combine(TemporaryFilePath, newFolderName)))
            {
                counter++;
                newFolderName = originalName + "_" + counter;
            }

            try
            {
                Directory.CreateDirectory(Path.Combine(TemporaryFilePath, newFolderName));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Cannot Create Session Folder.");
            }

            _caches.Add(sessionName, Path.Combine(TemporaryFilePath, newFolderName));
            return newFolderName;
        }

        public string CreateTempFile(string sessionName, string extensionWithDot, string folders = null)
        {
            if (!_caches.ContainsKey(sessionName))
                throw new Exception("Invalid Session");


            string newFile = Path.Combine(_caches[sessionName], Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + extensionWithDot);
            return newFile;
        }

        public void DestroySession(string sessionName)
        {
            if (!_caches.ContainsKey(sessionName))
                return;

            bool keepTrying = true;
            while (keepTrying)
            {
                try
                {
                    if (Directory.Exists(_caches[sessionName]))
                        Directory.Delete(_caches[sessionName], true);
                    keepTrying = false;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }

            _caches.Remove(sessionName);
        }

        public string CreateDirectory(string sessionName, string directoryName)
        {
            if (!_caches.ContainsKey(sessionName))
                throw new Exception("Invalid Session.");

            if (Directory.Exists(Path.Combine(_caches[sessionName], directoryName)))
                throw new Exception("Directory Exists.");

            Directory.CreateDirectory(Path.Combine(_caches[sessionName], directoryName));

            return Path.Combine(_caches[sessionName], directoryName);
        }

        public void Dispose()
        {
            foreach (string key in _caches.Keys.ToList())
            {
                DestroySession(key);
            }
        }
    }
}