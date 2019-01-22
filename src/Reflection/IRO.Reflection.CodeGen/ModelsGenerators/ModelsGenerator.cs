using System;
using System.Collections.Generic;
using System.Reflection;
using IRO.Common.Text;
using IRO.Reflection.CodeGen.Exceptions;
using IRO.Reflection.Core;

namespace IRO.Reflection.CodeGen.ModelsGenerators
{
    public class ModelsGenerator
    {
        readonly IEnumerable<ModelsGeneratorInput> _modelsGeneratorInputs;
        readonly string _namespaceStr;
        readonly string _baseClassName;
        readonly bool _typeNamesWithAssembly;

        protected SourceFileContext CurrentContext { get; private set; }

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
                NewContext();
                var modelsScript = "";
                foreach (var param in _modelsGeneratorInputs)
                {
                    modelsScript += GenerateModelSourceCode(param) + "\n";
                }

                var usings = CurrentContext.GetNamespaces();
                if (usings.Contains(_namespaceStr))
                {
                    usings.Remove(_namespaceStr);
                }
                modelsScript = CodeGenExtensions.WrapClass(modelsScript, _namespaceStr, usings);

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

        protected virtual string GenerateModelSourceCode(ModelsGeneratorInput modelsGeneratorInput)
        {
            var modelName = modelsGeneratorInput.ClassName;
            string newClassStr = "public class " +
                modelName +
                GenerateBaseClass(modelsGeneratorInput) +
                "\n{\n";
            foreach (var param in modelsGeneratorInput.Params)
            {
                var paramName = param.ParamName;
                paramName = paramName[0].ToString().ToUpper() + paramName.Substring(1);
                var attrs = GenerateAttributes(param.ParamInfo);
                var newClassProp = attrs
                    + "public "
                    + GetTypeName(param.ParamInfo.ParameterType)
                    + " "
                    + paramName
                    + " { get; set; }";
                newClassStr += "\n" + TextExtensions.AddTabs(newClassProp, 1) + "\n";
            }
            newClassStr += "\n}";
            return newClassStr;
        }

        public void NewContext()
        {
            CurrentContext = new SourceFileContext();
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
