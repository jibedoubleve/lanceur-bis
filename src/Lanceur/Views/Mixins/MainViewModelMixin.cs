﻿using Lanceur.Core.Requests;

namespace Lanceur.Views.Mixins;

public static class MainViewModelMixin
{
    #region Methods

    public static AliasExecutionRequest BuildExecutionRequest(this MainViewModel viewModel, string query, bool runAsAdmin = false) => new() { Query = query, RunAsAdmin = runAsAdmin, AliasToExecute = viewModel.CurrentAlias };

    #endregion Methods
}