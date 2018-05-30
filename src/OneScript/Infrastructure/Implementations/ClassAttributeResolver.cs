using System.Collections.Generic;
using System.Linq;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    internal class ClassAttributeResolver :IDirectiveResolver
    {
        private readonly string[] _knownAnnotations;
        private List<AnnotationDefinition> _annotations;

        public ClassAttributeResolver()
        {
            _knownAnnotations = new[]
            {
                "Авторизовать",
                "Authorize"
            };
        }

        public bool Resolve(string directive, string value, bool codeEntered)
        {
            if (!_knownAnnotations.Any(directive.StartsWith))
                return false;

            if (codeEntered)
                throw new CompilerException("Директивы аннотаций должны предшествовать строкам кода");

            var annotation = new AnnotationDefinition();
            annotation.Name = directive;

            if (value != null)
            {
                annotation.Parameters = ParseAnnotationParameters(value);
            }

            _annotations.Add(annotation);

            return true;
        }

        private AnnotationParameter[] ParseAnnotationParameters(string value)
        {
            //TODO: сделать разбор параметров
            return new AnnotationParameter[0];
        }

        public ICodeSource Source { get; set; }

        public IEnumerable<AnnotationDefinition> Attributes => _annotations;

        public void BeforeCompilation()
        {
            _annotations = new List<AnnotationDefinition>();
        }

        public void AfterCompilation()
        {
        }
    }
}