using Lanceur.Core.Services;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Mixins;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lanceur.Ui
{
    public class ImageCache : IImageCache
    {
        #region Fields

        private ConcurrentDictionary<string, ImageSource> _cache;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public ImageCache(IAppLoggerFactory loggerFactory)
        {
            _log = loggerFactory.GetLogger<ImageSource>();
        }

        #endregion Constructors

        #region Indexers

        public ImageSource this[string idx]
        {
            get
            {
                var i = new FileInfo(idx).Name;
                return _cache[i];
            }
            set
            {
                var i = new FileInfo(idx).Name;
                _cache[i] = value;
            }
        }

        #endregion Indexers

        #region Methods

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, ImageSource>> GetEnumerator() => _cache.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public bool IsInCache(string path)
        {
            var name = new FileInfo(path).Name;
            return _cache.ContainsKey(name);
        }

        /// <inheritdoc />
        public void LoadCache()
        {
            if (_cache is not null) return;

            _cache = new();
            var timer = new Stopwatch();
            timer.Start();
            var images = (from file in Directory.GetFiles(AppPaths.ImageCache.ExpandPath())
                          where file.Contains(".png")
                          select file).ToArray();
            
            foreach (var image in images)
            {
                var fileInfo = new FileInfo(image);
                var fileName = fileInfo.Name.Replace(".png", "");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new(image, UriKind.Absolute);
                bitmap.EndInit();

                _cache[fileName] = bitmap;
            }
            timer.Stop();
            _log.Info($"Loaded image cache in {timer.ElapsedMilliseconds} msec.");
        }

        #endregion Methods
    }
}