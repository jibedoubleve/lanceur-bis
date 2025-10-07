namespace Lanceur.Ui.Core.Utils.ConnectionStrings;

public abstract class BaseConnectionString
{
    #region Fields

    protected const string ConnectionStringPattern = "Data Source={0};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache=Shared;Pooling=true;Max Pool Size=50;";

    #endregion Fields
}