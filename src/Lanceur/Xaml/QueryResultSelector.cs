using Lanceur.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace Lanceur.Xaml
{
    internal class QueryResultSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate DisplayResultTemplate { get; set; }

        #endregion Properties

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DisplayQueryResult => DisplayResultTemplate,
                QueryResult => DefaultTemplate,
                _ => DefaultTemplate,
            };
        }

        #endregion Methods
    }
}