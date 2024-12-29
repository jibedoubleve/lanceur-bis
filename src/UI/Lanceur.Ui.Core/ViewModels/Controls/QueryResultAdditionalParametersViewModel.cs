using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.ViewModels.Controls
{
    public partial class QueryResultAdditionalParametersViewModel : ObservableObject
    {
        private readonly AdditionalParameter _model;

        public string Name
        {
            get => _model.Name;
            set
            {
                _model.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Parameter
        {
            get => _model.Parameter;
            set
            {
                _model.Parameter = value;
                OnPropertyChanged(nameof(Parameter));
            }
        }

        public QueryResultAdditionalParametersViewModel(AdditionalParameter model)
        {
            this._model = model;
        }
    }
}
