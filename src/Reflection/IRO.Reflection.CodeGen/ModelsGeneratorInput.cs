using System.Collections.Generic;
using System.Reflection;
using IRO.Reflection.Core;

namespace IRO.Reflection.CodeGen
{
    public struct ModelsGeneratorInput
    {
        public string ModelName { get; set; }

        public List<Parameter> Params { get; set; }
    }
}
