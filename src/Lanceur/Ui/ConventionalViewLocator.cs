using ReactiveUI;
using Splat;
using System;

namespace Lanceur.Ui
{
    public class ConventionalViewLocator : IViewLocator
    {
        #region Methods

        public IViewFor ResolveView<T>(T viewModel, string contract = null)
        {
            // Find view's by chopping of the 'Model' on the view model name
            // MyApp.ShellViewModel => MyApp.ShellView
            var viewModelName = viewModel?.GetType().FullName;
            var viewTypeName = viewModelName?.TrimEnd("Model".ToCharArray())
                                            .TrimEnd("Document".ToCharArray()) ?? string.Empty;

            try
            {
                var viewType = Type.GetType(viewTypeName);
                if (viewType == null)
                {
                    this.Log().Error($"Could not find the view {viewTypeName} for view model {viewModelName}.");
                    return null;
                }
                this.Log().Info($"Find view {viewTypeName} for view model {viewModelName}.");
                return Activator.CreateInstance(viewType) as IViewFor;
            }
            catch (Exception)
            {
                this.Log().Error($"Could not instantiate view {viewTypeName}.");
                throw;
            }
        }

        #endregion Methods
    }
}