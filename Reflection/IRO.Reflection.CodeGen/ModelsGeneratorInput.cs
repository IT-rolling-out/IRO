using System.Collections.Generic;
using System.Reflection;

namespace IRO.Reflection.CodeGen
{
    public struct ModelsGeneratorInput
    {
        public string ModelName { get; set; }

        public List<Parameter> Params { get; set; }
    }
}
