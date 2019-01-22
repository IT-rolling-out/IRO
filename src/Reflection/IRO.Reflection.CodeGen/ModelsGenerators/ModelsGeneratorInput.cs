using System.Collections.Generic;
using IRO.Reflection.Core.ModelBinders;

namespace IRO.Reflection.CodeGen.ModelsGenerators
{
    public struct ModelsGeneratorInput
    {
        public string ClassName { get; set; }

        public List<Parameter> Params { get; set; }
    }
}
