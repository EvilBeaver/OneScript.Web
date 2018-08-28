using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public class AnnotationException : RuntimeException
    {
        public AnnotationException(AnnotationDefinition anno, string message) 
            : base($"Неверное применение аннотации {anno.Name}: {message}")
        {
        }
    }
}
