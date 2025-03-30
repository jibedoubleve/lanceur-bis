using System.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

/// <summary>
///     Defines a contract for managing database connections and transactions.
/// </summary>
public interface IDbConnectionManager : IDisposable
{
    #region Methods

    /// <summary>
    ///     Manages database connections and ensures proper disposal of resources.
    ///     This method creates a database connection and passes it to the provided action.
    /// </summary>
    /// <typeparam name="TReturn">
    ///     The type of the result returned by the action, typically the result of a database query.
    /// </typeparam>
    /// <param name="action">
    ///     A delegate that defines the operation to be performed using the provided <see cref="IDbConnection" />.
    ///     The connection is passed as a parameter to the action.
    /// </param>
    /// <returns>
    ///     The result of the operation performed by the action.
    ///     This could be the result of a database query or any other operation defined within the action.
    /// </returns>
    TReturn WithConnection<TReturn>(Func<IDbConnection, TReturn> action);
    
    /// <summary>
    ///     Manages database connections and ensures proper disposal of resources.
    ///     This method creates a database connection and passes it to the provided action.
    /// </summary>
    /// <param name="action">
    ///     A delegate that defines the operation to be performed using the provided <see cref="IDbConnection" />.
    ///     The connection is passed as a parameter to the action.
    /// </param>
    void WithConnection(Action<IDbConnection> action);

    /// <summary>
    ///     Executes an action within a database transaction.
    /// </summary>
    /// <param name="action">
    ///     The action to be executed. It accepts an <see cref="IDbTransaction" /> as a parameter.
    /// </param>
    /// <remarks>
    ///     The action provided will be executed within the context of a transaction.
    ///     If the action fails, the transaction is rolled back.
    /// </remarks>
    void WithinTransaction(Action<IDbTransaction> action);

    /// <summary>
    ///     Executes a function within a database transaction and returns a result.
    /// </summary>
    /// <typeparam name="TReturn">The type of the result returned by the function.</typeparam>
    /// <param name="action">
    ///     The function to be executed. It accepts an <see cref="IDbTransaction" /> and returns a result of type
    ///     <typeparamref name="TReturn" />.
    /// </param>
    /// <returns>
    ///     The result of the function executed within the transaction.
    /// </returns>
    /// <remarks>
    ///     The function provided will be executed within the context of a transaction.
    ///     If the function fails, the transaction is rolled back.
    /// </remarks>
    TReturn WithinTransaction<TReturn>(Func<IDbTransaction, TReturn> action);

    /// <summary>
    ///     Executes a function within a database transaction and passes a context object.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="action">
    ///     The function to be executed. It accepts an <see cref="IDbTransaction" /> and a context object of type
    ///     <typeparamref name="TContext" />,
    ///     and returns the modified context object.
    /// </param>
    /// <param name="context">
    ///     The context object to be passed to the function.
    /// </param>
    /// <returns>
    ///     The context object after the function has executed.
    /// </returns>
    /// <remarks>
    ///     The function provided will be executed within the context of a transaction.
    ///     If the function fails, the transaction is rolled back.
    /// </remarks>
    TContext WithinTransaction<TContext>(Func<IDbTransaction, TContext, TContext> action, TContext context);
    
    /// <summary>
    ///     Executes a function within a database transaction and passes a context object.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="action">
    ///     The function to be executed. It accepts an <see cref="IDbTransaction" /> and a context object of type
    ///     <typeparamref name="TContext" />,
    ///     and returns the modified context object.
    /// </param>
    /// <param name="context">
    ///     The context object to be passed to the function.
    /// </param>
    /// <remarks>
    ///     The function provided will be executed within the context of a transaction.
    ///     If the function fails, the transaction is rolled back.
    /// </remarks>
    void WithinTransaction<TContext>(Action<IDbTransaction, TContext> action, TContext context);

    #endregion
}