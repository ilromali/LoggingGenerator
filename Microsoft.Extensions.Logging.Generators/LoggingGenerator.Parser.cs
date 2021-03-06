// ? Microsoft Corporation. All rights reserved.

[assembly: System.Resources.NeutralResourcesLanguage("en-us")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Extensions.Logging.Generators.Tests")]

namespace Microsoft.Extensions.Logging.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public partial class LoggingGenerator
    {
        internal class Parser
        {
            private const string DiagnosticCategory = "LoggingGenerator";

#pragma warning disable RS2008 // Enable analyzer release tracking

            private static readonly DiagnosticDescriptor ErrorInvalidMethodName = new(
                id: "LG0",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidMethodNameTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidMethodNameMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorInvalidMessage = new(
                id: "LG1",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidMessageTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidMessageMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorInvalidParameterName = new(
                id: "LG2",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidParameterNameTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidParameterNameMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorNestedType = new(
                id: "LG3",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorNestedTypeTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorNestedTypeMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorMissingRequiredType = new(
                id: "LG4",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorMissingRequiredTypeTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorMissingRequiredTypeMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorEventIdReuse = new(
                id: "LG5",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorEventIdReuseTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorEventIdReuseMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorInvalidMethodReturnType = new(
                id: "LG6",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidMethodReturnTypeTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorInvalidMethodReturnTypeMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorFirstArgMustBeILogger = new(
                id: "LG7",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorFirstArgMustBeILoggerTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorFirstArgMustBeILoggerMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorNotStaticMethod = new(
                id: "LG8",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorNotStaticMethodTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorNotStaticMethodMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorNotPartialMethod = new(
                id: "LG9",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorNotPartialMethodTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorNotPartialMethodMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private static readonly DiagnosticDescriptor ErrorMethodIsGeneric = new(
                id: "LG10",
                title: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorMethodIsGenericTitle), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                messageFormat: new LocalizableResourceString(nameof(LoggingGeneratorResources.ErrorMethodIsGenericMessage), LoggingGeneratorResources.ResourceManager, typeof(LoggingGeneratorResources)),
                category: DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            private readonly CancellationToken _cancellationToken;
            private readonly Compilation _compilation;
            private readonly Action<Diagnostic> _reportDiagnostic;
            private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels = new();

            public Parser(GeneratorExecutionContext context) : this(context.Compilation, context.ReportDiagnostic, context.CancellationToken)
            {
            }

            public Parser(Compilation compilation, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
            {
                _compilation = compilation;
                _cancellationToken = cancellationToken;
                _reportDiagnostic = reportDiagnostic;
            }

            /// <summary>
            /// Gets the set of logging classes containing methods to output.
            /// </summary>
            public IEnumerable<LoggerClass> GetLogClasses(IEnumerable<ClassDeclarationSyntax> classes)
            {
                var results = new List<LoggerClass>(); 

                var loggerMessageAttribute = _compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessageAttribute");
                if (loggerMessageAttribute is null)
                {
                    Diag(ErrorMissingRequiredType, null, "Microsoft.Extensions.Logging.LoggerMessageAttribute");
                    return results;
                }

                var exSymbol = _compilation.GetTypeByMetadataName("System.Exception");
                if (exSymbol == null)
                {
                    Diag(ErrorMissingRequiredType, null, "System.Exception");
                    return results;
                }

                var loggerSymbol = _compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
                if (loggerSymbol == null)
                {
                    Diag(ErrorMissingRequiredType, null, "Microsoft.Extensions.Logging.ILogger");
                    return results;
                }

                var ids = new HashSet<string>();
                foreach (var classDef in classes)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        // be nice and stop if we're asked to
                        return results;
                    }

                    LoggerClass? lc = null;
                    ids.Clear();
                    foreach (var method in classDef.Members.Where(m => m.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>())
                    {
                        foreach (var mal in method.AttributeLists)
                        {
                            foreach (var ma in mal.Attributes)
                            {
                                var semanticModel = GetSemanticModel(ma.SyntaxTree);
                                var maSymbol = semanticModel.GetSymbolInfo(ma, _cancellationToken);
                                if (maSymbol.Symbol is IMethodSymbol ms && loggerMessageAttribute.Equals(ms.ContainingType, SymbolEqualityComparer.Default))
                                {
                                    var arg = ma.ArgumentList!.Arguments[0];
                                    var eventId = semanticModel.GetConstantValue(arg.Expression).ToString();

                                    arg = ma.ArgumentList!.Arguments[1];
                                    var level = (int)semanticModel.GetConstantValue(arg.Expression).Value!;

                                    arg = ma.ArgumentList!.Arguments[2];
                                    var message = semanticModel.GetConstantValue(arg.Expression).ToString();

                                    string methodName = method.Identifier.ToString();
                                    string eventName = string.Empty;

                                    if (ma.ArgumentList?.Arguments is { Count: > 3 } args)
                                    {
                                        arg = args[3];
                                        eventName = semanticModel.GetConstantValue(arg.Expression).ToString();
                                    }

                                    var lm = new LoggerMethod
                                    {
                                        Name = methodName,
                                        Level = level,
                                        Message = message,
                                        EventId = eventId,
                                        EventName = eventName,
                                        MessageHasTemplates = HasTemplates(message),
                                        Modifiers = method.Modifiers.ToString(),
                                    };

                                    bool keep = true;
                                    if (lm.Name.StartsWith("__", StringComparison.Ordinal))
                                    {
                                        // can't have logging method names that start with __ since that can lead to conflicting symbol names
                                        // because the generated symbols start with __
                                        Diag(ErrorInvalidMethodName, method.Identifier.GetLocation());
                                    }

                                    if (GetSemanticModel(method.ReturnType.SyntaxTree).GetTypeInfo(method.ReturnType!).Type!.SpecialType != SpecialType.System_Void)
                                    {
                                        Diag(ErrorInvalidMethodReturnType, method.ReturnType.GetLocation());
                                        keep = false;
                                    }

                                    if (method.Arity > 0)
                                    {
                                        Diag(ErrorMethodIsGeneric, method.Identifier.GetLocation());
                                        keep = false;
                                    }

                                    bool isStatic = false;
                                    bool isPartial = false;
                                    foreach (var mod in method.Modifiers)
                                    {
                                        switch (mod.Text)
                                        {
                                            case "partial":
                                                isPartial = true;
                                                break;

                                            case "static":
                                                isStatic = true;
                                                break;
                                        }
                                    }

                                    if (!isStatic)
                                    {
                                        Diag(ErrorNotStaticMethod, method.GetLocation());
                                        keep = false;
                                    }

                                    if (!isPartial)
                                    {
                                        Diag(ErrorNotPartialMethod, method.GetLocation());
                                        keep = false;
                                    }

                                    // ensure there are no duplicate ids.
                                    if (ids.Contains(lm.EventId))
                                    {
                                        Diag(ErrorEventIdReuse, ma.ArgumentList!.Arguments[0].GetLocation(), lm.EventId);
                                    }
                                    else
                                    {
                                        ids.Add(lm.EventId);
                                    }

                                    if (string.IsNullOrWhiteSpace(lm.Message))
                                    {
                                        Diag(ErrorInvalidMessage, ma.GetLocation(), method.Identifier.ToString());
                                    }

                                    bool first = true;
                                    foreach (var p in method.ParameterList.Parameters)
                                    {
                                        var pSymbol = GetSemanticModel(p.SyntaxTree).GetTypeInfo(p.Type!).Type!;
                                        var typeName = pSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));

                                        if (first)
                                        {
                                            // skip the ILogger
                                            first = false;

                                            if (!IsBaseOrIdentity(pSymbol, loggerSymbol))
                                            {
                                                Diag(ErrorFirstArgMustBeILogger, p.Identifier.GetLocation());
                                                keep = false;
                                            }

                                            lm.LoggerType = typeName;
                                            continue;
                                        }

                                        var lp = new LoggerParameter
                                        {
                                            Name = p.Identifier.ToString(),
                                            Type = typeName,
                                            IsExceptionType = IsBaseOrIdentity(pSymbol, exSymbol),
                                        };
                                                    
                                        lm.Parameters.Add(lp);

                                        if (lp.Name.StartsWith("__", StringComparison.Ordinal))
                                        {
                                            // can't have logging method parameter names that start with  __ since that can lead to conflicting symbol names
                                            // because all generated symbols start with __
                                            Diag(ErrorInvalidParameterName, p.Identifier.GetLocation());
                                        }
                                    }

                                    if (keep)
                                    {
                                        if (lc == null)
                                        {
                                            // determine the namespace the class is declared in, if any
                                            NamespaceDeclarationSyntax? ns = null;
                                            if (classDef.Parent != null)
                                            {
                                                ns = classDef.Parent as NamespaceDeclarationSyntax;
                                                if (ns == null && classDef.Parent is not CompilationUnitSyntax)
                                                {
                                                    // since this generator doesn't know how to generate a nested type...
                                                    Diag(ErrorNestedType, classDef.Identifier.GetLocation());
                                                    continue;
                                                }
                                            }

                                            lc = new LoggerClass
                                            {
                                                Namespace = ns?.Name.ToString(),
                                                Name = classDef.Identifier.ToString(),
                                                Constraints = classDef.ConstraintClauses.ToString(),
                                            };

                                            if (classDef.TypeParameterList != null)
                                            {
                                                lc.Name += classDef.TypeParameterList.ToString();
                                            }
                                        }

                                        lc.Methods.Add(lm);
                                    }
                                }
                            }
                        }
                    }

                    if (lc != null)
                    {
                        results.Add(lc);
                    }
                }

                return results;
            }

            private void Diag(DiagnosticDescriptor desc, Location? location, params object?[]? messageArgs)
            {
                _reportDiagnostic(Diagnostic.Create(desc, location, messageArgs));
            }

            // Workaround for https://github.com/dotnet/roslyn/pull/49330
            private SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
            {
                if (!_semanticModels.TryGetValue(syntaxTree, out var semanticModel))
                {
                    semanticModel = _compilation.GetSemanticModel(syntaxTree);
                    _semanticModels[syntaxTree] = semanticModel;
                }

                return semanticModel;
            }

            private bool IsBaseOrIdentity(ITypeSymbol source, ITypeSymbol dest)
            {
                var conversion = _compilation.ClassifyConversion(source, dest);
                return conversion.IsIdentity || (conversion.IsReference && conversion.IsImplicit);
            }

            /// <summary>
            /// Does the string contain templates?
            /// </summary>
            private static bool HasTemplates(string message)
            {
                for (int i = 0; i < message.Length; i++)
                {
                    var ch = message[i];
                    if (ch == '{')
                    {
                        if (i < message.Length - 1 && message[i + 1] != '{')
                        {
                            // look for a non-escaped }
                            i++;
                            for (; i < message.Length; i++)
                            {
                                ch = message[i];
                                if (ch == '}')
                                {
                                    if (i == message.Length - 1 || message[i + 1] != '}')
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }
                    }
                }

                return false;
            }
        }

#pragma warning disable SA1401 // Fields should be private

        /// <summary>
        /// A logger class holding a bunch of logger methods.
        /// </summary>
        internal class LoggerClass
        {
            public string? Namespace;
            public string Name = string.Empty;
            public string Constraints = string.Empty;
            public List<LoggerMethod> Methods = new();
        }

        /// <summary>
        /// A logger method in a logger class.
        /// </summary>
        internal class LoggerMethod
        {
            public string Name = string.Empty;
            public string Message = string.Empty;
            public int Level;
            public string EventId = string.Empty;
            public string EventName = string.Empty;
            public bool MessageHasTemplates;
            public string Modifiers = string.Empty;
            public string LoggerType = string.Empty;
            public List<LoggerParameter> Parameters = new();
        }

        /// <summary>
        /// A single parameter to a logger method.
        /// </summary>
        internal class LoggerParameter
        {
            public string Name = string.Empty;
            public string Type = string.Empty;
            public bool IsExceptionType;
        }
    }
}
