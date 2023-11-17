using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Images;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;

namespace Lanceur.Ui.Thumbnails
{
    public class WPFThumbnailManager : IThumbnailManager
    {
        #region Fields

        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public WPFThumbnailManager(IAppLoggerFactory loggerFactory)
        {
            _log = loggerFactory.GetLogger<WPFThumbnailManager>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Launch a thread to refresh the thumbnails and returns just after. Each time an thumbnail is found
        /// the alias is updated and (because the alias is reactive) the UI should be updated.
        /// </summary>
        /// <remarks>
        /// All the alias are updated at once to avoid concurrency issues.Thumbnail
        /// </remarks>
        /// <param name="queries">The list a queries that need to have an updated thumbnail.</param>
        public void RefreshThumbnails(IEnumerable<QueryResult> queries)
        {
            try
            {
                Parallel.ForEach(queries, query =>
                {
                    if (query is not AliasQueryResult alias) return;
                    if (alias.FileName.IsNullOrEmpty()) return;

                    if (alias.IsPackagedApplication())
                    {
                        alias.Thumbnail.CopyToImageRepository(alias.FileName);
                        return;
                    }

                    var imageSource = ThumbnailLoader.GetThumbnail(alias.FileName);
                    if (imageSource is null)
                    {
                        alias.Thumbnail = null;
                        if (!alias.FileName.IsUrl()) return;

                        var favicon = alias.FileName
                                           .GetKeyForFavIcon()
                                           .ToAbsolutePath();
                        if (File.Exists(favicon))
                        {
                            alias.Thumbnail = favicon;
                        }
                        else
                        {
                            alias.Icon = "Web";
                        }

                        return;
                    }

                    var file = new FileInfo(alias.FileName);
                    imageSource.CopyToImageRepository(file.Name);
                    alias.Thumbnail = file.Name.ToAbsolutePath();
                });
            }
            catch(Exception ex)
            {
                _log.Warning($"An error occured during the refresh of the icons. ('{ex.Message}')", ex);
            }
        }

        #endregion Methods
    }
}