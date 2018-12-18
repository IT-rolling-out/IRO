using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace ItRollingOut.Tools.PureBinding.Base
{
    /// <summary>
    /// Пустой биндер моделей, нужен лишь как отметка для аттрибутов, наследующих ModelBinder.
    /// </summary>
    public class EmptyModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            
        }
    }
}
