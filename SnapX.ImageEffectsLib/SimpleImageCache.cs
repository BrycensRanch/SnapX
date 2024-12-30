using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SnapX.Core;

namespace SnapX.ImageEffectsLib;

public class SimpleImageCache : IDisposable
{
    private Dictionary<string, Image> _cache = new();
    private bool _disposed;

    // Max cache size (optional, can be set in the constructor)
    private int _maxCacheSize;

    public SimpleImageCache(int maxCacheSize = 100)
    {
        _maxCacheSize = maxCacheSize;
    }
    public Image GetImage(string filePath)
    {
        if (_cache.ContainsKey(filePath))
        {
            return _cache[filePath];
        }

        if (File.Exists(filePath))
        {
            try
            {
                var image = Image.Load(filePath);
                if (_cache.Count >= _maxCacheSize)
                {
                    var oldestKey = _cache.Keys.First();
                    _cache.Remove(oldestKey);
                }
                _cache[filePath] = image;
                return image;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Error loading image: {filePath}, Exception: {ex.Message}");
                return null;
            }
        }

        return null;
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    // Dispose method to clean up resources
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var image in _cache.Values)
                {
                    image.Dispose();
                }
                _cache.Clear();
            }

            // Mark as disposed
            _disposed = true;
        }
    }

    // Finalizer (destructor) to ensure resources are cleaned up if Dispose isn't called
    ~SimpleImageCache()
    {
        Dispose(false);
    }
}
