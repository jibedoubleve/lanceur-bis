using Lanceur.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lanceur.Core.Managers
{
    public interface IThumbnailManager
    {
        /// <summary>
        /// Asynchronously refresh the thumbnails. The methods lauches the threads
        /// and returns. Callbacks will set the property <see cref="QueryResult.Thumbnail"/>
        /// when thread has done its work
        /// </summary>
        /// <param name="queries">The <see cref="QueryResult"/> to refresh</param>
        void RefreshThumbnails(IEnumerable<QueryResult> queries);
    }
}
