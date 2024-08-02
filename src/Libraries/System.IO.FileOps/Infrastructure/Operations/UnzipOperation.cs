using System.IO.Compression;
using System.IO.FileOps.Core;

namespace System.IO.FileOps.Infrastructure.Operations;

internal class UnzipOperation : AbstractOperation, IOperation
{
    #region Constructors

    public UnzipOperation(Dictionary<string, string> parameters) : base("unzip", parameters) { }

    #endregion Constructors

    #region Properties

    private string ArchiveFile => Parameters["zip"];

    private string Destination => Parameters["destination"];

    #endregion Properties

    #region Methods

    public async Task ProcessAsync()
    {
        if (!File.Exists(ArchiveFile)) return;

        if (!Directory.Exists(Destination)) Directory.CreateDirectory(Destination);

        if (!Directory.Exists(Destination))
        {
            Directory.Delete(Destination, true);
            Directory.CreateDirectory(Destination);
        }

        await Task.Run(() => ZipFile.ExtractToDirectory(ArchiveFile, Destination));
    }

    #endregion Methods
}