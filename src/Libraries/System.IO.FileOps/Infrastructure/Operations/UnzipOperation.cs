using System.IO.Compression;
using System.IO.FileOps.Core;

namespace System.IO.FileOps.Infrastructure.Operations;

internal class UnzipOperation : AbstractOperation, IOperation
{
    #region Private members

    private string ArchiveFile => Parameters["zip"];

    private string Destination => Parameters["destination"];

    #endregion

    #region Constructors

    public UnzipOperation(Dictionary<string, string> parameters) : base("unzip", parameters) { }

    #endregion

    #region Public methods

    public async Task ProcessAsync()
    {
        if (!File.Exists(ArchiveFile)) return;

        if (!Directory.Exists(Destination))
        {
            Directory.CreateDirectory(Destination);
            
        }

        if (!Directory.Exists(Destination))
        {
            Directory.Delete(Destination, recursive: true);
            Directory.CreateDirectory(Destination);
        }

        await Task.Run(() => ZipFile.ExtractToDirectory(ArchiveFile, Destination));
    }

    #endregion
}