using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;
using NSwag.Generation.WebApi;
using System;
using System.Collections.Generic;

namespace NswagBug
{

    class Program
    {

        static string JsonSerializerParameterCodeLogic(CSharpClientGeneratorSettings _settings)
        {
            bool RequiresJsonExceptionConverter = true;

            var parameterCode = 
                CSharpJsonSerializerGenerator.GenerateJsonSerializerParameterCode(
                    _settings.CSharpGeneratorSettings, RequiresJsonExceptionConverter ? 
                    new[] { "JsonExceptionConverter" } : null);


            if (string.IsNullOrEmpty(parameterCode))
            {
                parameterCode = true ?
                    "new Newtonsoft.Json.JsonSerializerSettings()" :
                    "new System.Text.Json.JsonSerializerOptions()";
            }
            else if (!parameterCode.Contains("new Newtonsoft.Json.JsonSerializerSettings"))
            {
                parameterCode = true ?
                    "new Newtonsoft.Json.JsonSerializerSettings { Converters = " + parameterCode.Substring(2) + " }" :
                    parameterCode.Substring(2);
            }
            else
            {
                parameterCode = parameterCode.Substring(2);
            }

            return parameterCode;
        }

        static void Main(string[] args)
        {
            var st = new CSharpClientGeneratorSettings
            {
                CSharpGeneratorSettings =
                    {
                        Namespace = "Somenamespace",
                        JsonLibrary= CSharpJsonLibrary.NewtonsoftJson
                    },
                ExceptionClass = "UnexpectedApiException",
                GenerateClientInterfaces = true,
                GenerateDtoTypes = true,
                GenerateClientClasses = true,
            };

            
            string buggedString = JsonSerializerParameterCodeLogic(st);

            var settings = new WebApiOpenApiDocumentGeneratorSettings()
            {
                Title = "Title",
                IsAspNetCore = false,
                Description = "C# client",
                SchemaType = NJsonSchema.SchemaType.Swagger2,
                Version = "1.0.0.0",

            };

            var generator = new WebApiOpenApiDocumentGenerator(settings);

            NSwag.OpenApiDocument document = generator.GenerateForControllersAsync(
                new List<System.Type> { typeof(WebSite.Controllers.ValuesController) }
                ).Result;

            

            var codeGen = new CSharpClientGenerator(document,st);


            //bugged code produced
            //with
            // var settings = w Newtonsoft.Json.JsonSerializerSettings();
            Console.WriteLine(codeGen.GenerateFile());
        }
    }
}
