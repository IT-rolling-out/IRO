using System.Collections.Generic;
using System.Reflection;

namespace ItRollingOut.Reflection.CodeGen
{
    public struct ModelsGeneratorInput
    {
        public string ModelName { get; set; }

        public List<Parameter> Params { get; set; }
    }
}
