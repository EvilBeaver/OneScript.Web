/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    internal class ClassAttributeResolver : ModuleAnnotationDirectiveHandler
    {
        private readonly string[] _knownAnnotations;
        private List<AnnotationDefinition> _annotations;

        public ClassAttributeResolver(IAstBuilder nodeBuilder, IErrorSink errorSink): base(nodeBuilder, errorSink)
        {
            _knownAnnotations = new[]
            {
                "Авторизовать",
                "Authorize"
            };
        }

        public ICodeSource Source { get; set; }

        protected override bool DirectiveSupported(string directive) => 
            _knownAnnotations.Any(x => directive.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));

        protected override void ParseAnnotationInternal(ref Lexem lastExtractedLexem, ILexer lexer)
        {
            var annotation = NodeBuilder.CreateNode(NodeKind.Annotation, lastExtractedLexem);
            NodeBuilder.AddChild(NodeBuilder.CurrentNode, annotation);
            
            lastExtractedLexem = lexer.NextLexemOnSameLine();
            if (lastExtractedLexem.Type != LexemType.EndOfText)
            {
                ErrorSink.AddError(new ParseError()
                {
                    Description = "Неверное объявление атрибута класса",
                    ErrorId = nameof(ClassAttributeResolver),
                    Position = lexer.GetErrorPosition()
                });
            }

            lastExtractedLexem = lexer.NextLexem();
        }
    }
}
