using IRO.Reflection.SummarySearch;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IRO.Mvc.CoolSwagger
{

    public class SummaryOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            try
            {
                var xml = DocsParser.XmlFromMethod(context.MethodInfo);
                string summaryText =
                    DocsParserExtensions.XmlSummaryToString(xml);
                if (!string.IsNullOrWhiteSpace(summaryText))
                    operation.Summary = summaryText;
                summaryText += ".\n" + DocsParserExtensions.GetParamsText(xml);
                if (!string.IsNullOrWhiteSpace(summaryText))
                    operation.Description = summaryText;
            }
            catch { }
        }
    }
}
