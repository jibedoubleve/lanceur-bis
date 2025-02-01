using System.Web.Bookmarks.Domain;
using Everything.Wrapper;
using Humanizer;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Extensions;

public static class AliasQueryResultMapperExtensions
{
    #region Fields

    private const int Length = 66;

    #endregion

    #region Methods

    private static string GetIconForEverythingResult(ResultType itemResultType) => itemResultType switch
    {
        ResultType.File       => "Document24",
        ResultType.Excel      => "DocumentData24",
        ResultType.Pdf        => "DocumentPdf24",
        ResultType.Zip        => "FolderZip24",
        ResultType.Image      => "Image24",
        ResultType.Word       => "CodeTextEdit24",
        ResultType.Directory  => "Folder24",
        ResultType.Music      => "MusicNote224",
        ResultType.Text       => "DocumentText24",
        ResultType.Code       => "CodeBlock24",
        ResultType.Executable => "Run24",
        _                     => "AppGeneric24"
    };

    /// <summary>
    ///     Converts a <see cref="Bookmark" /> to an <see cref="AliasQueryResult" />.
    /// </summary>
    /// <param name="bookmark">The bookmark to be converted.</param>
    /// <returns>An <see cref="AliasQueryResult" /> representing the bookmark.</returns>
    public static AliasQueryResult ToAliasQueryResult(this Bookmark bookmark) => new()
    {
        Name = bookmark.Name.Truncate(Length, "(...)"),
        FileName = bookmark.Url,
        Count = -1,
        Icon = "Link24"
    };

    /// <summary>
    ///     Converts a <see cref="EverythingResult" /> to an <see cref="AliasQueryResult" />.
    /// </summary>
    /// <param name="everythingResult">The result to convert.</param>
    public static AliasQueryResult ToAliasQueryResult(this EverythingResult everythingResult) => new()
    {
        Name = everythingResult.Name.Truncate(Length, "(...)"),
        FileName = everythingResult.Path,
        Icon = GetIconForEverythingResult(everythingResult.ResultType),
        Thumbnail = null,
        IsThumbnailDisabled = true,
        Count = -1
    };

    #endregion
}