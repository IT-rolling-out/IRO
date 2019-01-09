using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IRO.Common.Services;
using IRO.Reflection.Core;

namespace IRO.Reflection.CodeGen
{
    public class ModelsGenerator
    {
        readonly IEnumerable<ModelsGeneratorInput> _modelsGeneratorInputs;
        readonly string _namespaceStr;
        readonly string _baseClassName;
        readonly bool _typeNamesWithAssembly;

        protected CsFileContext CurrentContext { get; private set; }

        public ModelsGenerator(
            IEnumerable<ModelsGeneratorInput> modelsGeneratorInputs,
            string namespaceStr,
            string baseClassName = null,
            bool typeNamesWithAssembly=false
            )
        {
            _modelsGeneratorInputs = modelsGeneratorInputs;
            _namespaceStr = namespaceStr.Trim();
            _baseClassName = baseClassName?.Trim();
            _typeNamesWithAssembly = typeNamesWithAssembly;
        }

        public CodeGenResult Generate()
        {
            try
            {
                ClearContext();
                var modelsScript = "";
                foreach (var param in _modelsGeneratorInputs)
                {
                    modelsScript += GenerateModel(param) + "\n";
                }

                var usings = CurrentContext.GetNamespaces();
                if (usings.Contains(_namespaceStr))
                {
                    usings.Remove(_namespaceStr);
                }
                modelsScript = SimpleGenerators.WrapClass(modelsScript, _namespaceStr, usings);

                var res = new CodeGenResult
                {
                    Context = CurrentContext,
                    CSharpCode = modelsScript
                };
                return res;
            }
            catch (Exception ex)
            {
                throw new CodeGenException($"Exception in {GetType().Name}.", ex);
            }
        }

        public virtual string GenerateModel(ModelsGeneratorInput modelsGeneratorInput)
        {
            var modelName = modelsGeneratorInput.ModelName;
            string newClassStr = "public class " +
                modelName +
                GenerateBaseClass(modelsGeneratorInput) +
                "\n{\n";
            foreach (var param in modelsGeneratorInput.Params)
            {
                var paramName = param.ParamName;
                paramName = paramName[0].ToString().ToUpper() + paramName.Substring(1);
                var attrs = GenerateAttributes(param.Info);
                var newClassProp = attrs
                    + "public "
                    + GetTypeName(param.Info.ParameterType)
                    + " "
                    + paramName
                    + " { get; set; }";
                newClassStr += "\n" + TextExtensions.AddTabs(newClassProp, 1) + "\n";
            }
            newClassStr += "\n}";
            return newClassStr;
        }

        public void ClearContext()
        {
            CurrentContext = new CsFileContext();
        }

        protected virtual string GenerateAttributes(ParameterInfo param)
        {
            return "";
        }

        protected string GetTypeName(Type t)
        {
            CurrentContext.UsedType(t);
            return t.GetNormalTypeName(_typeNamesWithAssembly);
        }

        string GenerateBaseClass(ModelsGeneratorInput modelsGeneratorInput)
        {
            if (string.IsNullOrWhiteSpace(_baseClassName))
                return "";
            return " : " + _baseClassName;
        }         
    }
}
