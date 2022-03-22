using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using SpotifyAPI.Web;
using System.ComponentModel;

namespace Lanceur.Plugins.JBPackage
{
    [Plugin("ss"), Description("Show current song played in spotify")]
    public class SpotifyPlugin : PluginBase
    {
        public override async Task<IEnumerable<QueryResult>> ExecuteAsync(string parameters = null)
        {
            var token = "BQDJ65YvouU9JY51g_hiAFC9OIn7NApQDutQxmSGlXDrrxpPVb5LZBJglGtOA9p6DFIK0fDA1WvOdQYd80oEgXpiRJLylETABWO6m_RV4a3z_Z0bfHoQSp0lB2uJs7gqX7bHIev1mvFKxrh4rwvM-v8H-8vOgM-W_kl-gUcP";
            var spotify = new SpotifyClient(token);

            var track = await spotify.Tracks.Get("1s6ux0lNiTziSrd7iUAADH");

            var list = new List<QueryResult>();
            var item = new AliasQueryResult
            {
                Name = $"{track.Name} ({track.Album})",
            };
            item.SetDescription(string.Join(", ", track.Artists));
            return list;

        }
    }
}