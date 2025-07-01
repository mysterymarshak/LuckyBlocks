using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using SFDGameScriptInterface;

namespace LuckyBlocks.SourceGenerators.ExtendedEvents;

[Generator]
public class ExtendedEventsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var extendedEvents = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax,
            static (gsc, _) =>
            {
                var node = (ClassDeclarationSyntax)gsc.Node;
                var typeInfo =
                    (ITypeSymbol)gsc.SemanticModel.GetDeclaredSymbol(node, CancellationToken.None)!;
                return typeInfo.Interfaces.Any(x => x.Name == "IExtendedEvents") ? typeInfo : null;
            }).Where(static x => x is not null);

        var compilationsAndClassDeclarations = context.CompilationProvider.Combine(extendedEvents.Collect());

        context.RegisterSourceOutput(compilationsAndClassDeclarations,
            static (spc, source) => Execute(source.Right!, spc));
    }

    private static void Execute(ImmutableArray<ITypeSymbol> declarations, SourceProductionContext context)
    {
        foreach (var extendedEvents in declarations)
        {
            BuildExtendedEvents(extendedEvents, context);
        }
    }

    private static void BuildExtendedEvents(ITypeSymbol extendedEvents, SourceProductionContext context)
    {
        const string folder = "ExtendedEvents";
        const string file = "ExtendedEvents.sbn-cs";

        var model = CreateExtendedEventsModel(extendedEvents, context);
        var path = Path.Combine(folder, "resources", file);
        var template = Template.Parse(EmbeddedResource.GetContent(path), path);

        // var attribute = declarations[1];
        // var members = attribute.GetMembers();
        // var method = ((IPropertySymbol)members[0]).GetMethod;
        //
        // var location = method!.Locations[0];
        // var root = location.SourceTree.GetRoot();
        // var expressionSyntax = (ParenthesizedLambdaExpressionSyntax)root.FindNode(location.SourceSpan);
        // var isStatic = expressionSyntax.Modifiers.SingleOrDefault(x => x.IsKind(SyntaxKind.StaticKeyword)) != default;
        // filters:

        AddSource(template, model, file, context);
    }

    private static ExtendedEventsModel CreateExtendedEventsModel(ITypeSymbol extendedEvents,
        SourceProductionContext context)
    {
        var @namespace = extendedEvents.ContainingNamespace.ToDisplayString();

        var implementedInterface = extendedEvents.Interfaces
            .First(x => x.Name == "IExtendedEvents");
        var methodsToImplement = implementedInterface
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(x => x.HasAttribute("GameCallbackTypeAttribute") || x.HasAttribute("GameCallbackType"))
            .ToList();

        var buildMethodResults = methodsToImplement
            .Select(BuildMethod)
            .ToList();

        var callbacksStorageModels = new List<CallbacksStorageModel>();
        var hookMethodModels = new List<HookMethodModel>();

        foreach (var result in buildMethodResults)
        {
            var callbacksStorageModel = BuildCallbacksStorageModel(result.CallbackArgumentTypes,
                result.FilterObjectType, result.GameCallbackType, result.GameCallbackTypeArguments);
            callbacksStorageModels.Add(callbacksStorageModel);

            var hookMethodModel =
                BuildHookMethodModel(result.HookMethodName, result.CallbackParameters, callbacksStorageModel);
            hookMethodModels.Add(hookMethodModel);
        }

        callbacksStorageModels = callbacksStorageModels
            .Distinct(new CallbacksStorageModelEqualityComparer())
            .ToList();

        var gameCallbackModels = buildMethodResults
            .Select(result => BuildGameCallbacksModel(result.GameCallbackType, result.GameCallbackTypeArguments,
                callbacksStorageModels.ToImmutableArray()))
            .ToList();

        gameCallbackModels = gameCallbackModels
            .Distinct(new GameCallbackModelEqualityComparer())
            .ToList();

        var gameCallbackMethodModels = gameCallbackModels
            .Select(BuildGameCallbackMethodModel)
            .ToList();

        return new ExtendedEventsModel(@namespace, "ExtendedEvents", callbacksStorageModels.ToImmutableArray(),
            gameCallbackModels.ToImmutableArray(),
            hookMethodModels.ToImmutableArray(), gameCallbackMethodModels.ToImmutableArray());
    }

    private static CallbacksStorageModel BuildCallbacksStorageModel(ImmutableArray<ITypeSymbol> callbackArgumentTypes,
        ITypeSymbol? filterObjectType, Type gameCallbackType, Type[] gameCallbackArgumentTypes)
    {
        var callbackTypeGenericArguments = string.Join(", ",
            callbackArgumentTypes.Select(x => x.GetUnderlyingName() + (x is IArrayTypeSymbol ? "[]" : string.Empty)));
        var callbackTypeArguments = string.IsNullOrWhiteSpace(callbackTypeGenericArguments)
            ? string.Empty
            : $"<{callbackTypeGenericArguments}>";
        var callbackType = $"Callback{callbackTypeArguments}";
        var type = filterObjectType is null
            ? $"List<{callbackType}>"
            : $"Dictionary<{filterObjectType.Name}, List<{callbackType}>>";
        return new(
            gameCallbackType.Name + "s" + (filterObjectType is null ? string.Empty : "Filtered") +
            callbackArgumentTypes.Sum(x => x.Name.Length), type, gameCallbackType, callbackArgumentTypes,
            callbackTypeArguments, callbackType, filterObjectType is not null, gameCallbackArgumentTypes);
    }

    private static GameCallbackModel BuildGameCallbacksModel(Type gameCallbackType, Type[] gameCallbackArguments,
        ImmutableArray<CallbacksStorageModel> callbackStorages)
    {
        var name = gameCallbackType.Name + gameCallbackArguments.Sum(x => x.Name.Length);
        var type = gameCallbackType.FullName!.Replace('+', '.')["SFDGameScriptInterface.".Length..];
        var callbackMethodName = $"On{name}";
        var linkedCallbacks = callbackStorages
            .Where(x =>
                x.GameCallbackType == gameCallbackType &&
                x.ArgumentTypes.All(y =>
                    gameCallbackArguments.Any(z => z.GetUnderlyingName() == y.GetUnderlyingName())))
            .ToImmutableArray();
        return new(name, type, callbackMethodName, gameCallbackArguments.ToImmutableArray(),
            gameCallbackArguments.First().IsArray, linkedCallbacks);
    }

    private static HookMethodModel BuildHookMethodModel(string name,
        ImmutableArray<IParameterSymbol> callbackParameters, CallbacksStorageModel callbacksStorageModel)
    {
        var signature = new HookMethodSignatureModel(name, callbackParameters);

        var delegateParameter = callbackParameters.First().IsDelegate() ? callbackParameters[0] : callbackParameters[1];
        var isFunc = delegateParameter.Type.Name == "Func";
        var callbackCtorParameters =
            isFunc
                ? "_instanceId, default, callbackDelegate, hookMode, ignoreHandled"
                : "_instanceId, callbackDelegate, default, hookMode, ignoreHandled";

        var body = new HookMethodBodyModel(callbackCtorParameters, callbacksStorageModel);

        return new(signature, body);
    }

    private static GameCallbackMethodModel BuildGameCallbackMethodModel(GameCallbackModel gameCallback)
    {
        var signature = new GameCallbackMethodSignatureModel(gameCallback);

        var body = new GameCallbackMethodBodyModel(gameCallback);

        return new(signature, body);
    }

    private static BuildMethodResult BuildMethod(IMethodSymbol methodSymbol)
    {
        var methodDeclarationSyntax =
            (MethodDeclarationSyntax)methodSymbol.DeclaringSyntaxReferences.First().GetSyntax();
        var parameters = methodSymbol.Parameters;

        var gameCallbackTypeAttributeArgument = methodDeclarationSyntax
            .AttributeLists
            .SelectMany(x => x.Attributes)
            .First(x => x.Name is IdentifierNameSyntax { Identifier.ValueText: "GameCallbackType" })
            .ArgumentList!
            .Arguments
            .First(
                x =>
                    x.Expression is TypeOfExpressionSyntax
                    {
                        Type: QualifiedNameSyntax
                        {
                            Left: IdentifierNameSyntax { Identifier.ValueText: nameof(Events) },
                            Right: IdentifierNameSyntax
                            {
                                Identifier.ValueText: nameof(Events.ObjectDamageCallback)
                                or nameof(Events.ObjectCreatedCallback) or nameof(Events.ObjectTerminatedCallback)
                                or nameof(Events.PlayerDamageCallback) or nameof(Events.ExplosionHitCallback)
                                or nameof(Events.PlayerDeathCallback) or nameof(Events.ProjectileCreatedCallback)
                                or nameof(Events.ProjectileHitCallback) or nameof(Events.PlayerKeyInputCallback)
                                or nameof(Events.PlayerMeleeActionCallback)
                                or nameof(Events.PlayerWeaponAddedActionCallback)
                                or nameof(Events.PlayerWeaponRemovedActionCallback)
                                or nameof(Events.UserMessageCallback) or nameof(Events.UpdateCallback)
                                or nameof(Events.PlayerCreatedCallback) or nameof(Events.UserJoinCallback)
                                or nameof(Events.UserLeaveCallback)
                            }
                        }
                    });

        var gameCallbackType = GetGameCallbackType(gameCallbackTypeAttributeArgument);
        var filterObjectType = GetFilterObject(parameters);
        var callbackArgumentTypes = GetCallbackArgumentTypes(parameters);
        var getGameCallbackMethodResult =
            GetGameCallbackMethod(gameCallbackType, filterObjectType, callbackArgumentTypes);
        var gameCallbackArguments = getGameCallbackMethodResult.Item2;

        return new BuildMethodResult(methodSymbol.Name, parameters, gameCallbackType, filterObjectType,
            callbackArgumentTypes, gameCallbackArguments);
    }

    private static Type GetGameCallbackType(AttributeArgumentSyntax gameCallbackTypeAttributeArgument)
    {
        var typeofExpression =
            (QualifiedNameSyntax)((TypeOfExpressionSyntax)gameCallbackTypeAttributeArgument.Expression).Type;
        var callbackTypeName = ((IdentifierNameSyntax)typeofExpression.Left).Identifier.ValueText + '.' +
                               ((IdentifierNameSyntax)typeofExpression.Right).Identifier.ValueText;
        return typeof(Events).Assembly.GetType($"{typeof(Events).Namespace}.{callbackTypeName.Replace('.', '+')}");
    }

    private static ITypeSymbol? GetFilterObject(ImmutableArray<IParameterSymbol> parameters)
    {
        var filterObjectParameter = parameters[0].IsDelegate() ? null : parameters[0];
        return filterObjectParameter?.Type;
    }

    private static ImmutableArray<ITypeSymbol> GetCallbackArgumentTypes(ImmutableArray<IParameterSymbol> parameters)
    {
        var callbackParameter = parameters[0].IsDelegate() ? parameters[0] : parameters[1];

        var typeArguments = ((INamedTypeSymbol)callbackParameter.Type).TypeArguments;
        if (typeArguments.IsDefaultOrEmpty)
        {
            return ImmutableArray<ITypeSymbol>.Empty;
        }

        var eventArgument = (INamedTypeSymbol)typeArguments.First();
        return eventArgument.TypeArguments;
    }

    private static (MethodInfo?, Type[]) GetGameCallbackMethod(Type gameCallbackType, ITypeSymbol? filterObjectType,
        ImmutableArray<ITypeSymbol> callbackArguments)
    {
        var methodsToScan = gameCallbackType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.Name == "Start");

        var methodsWithCallbackArguments = methodsToScan
            .Select(methodInfo => (methodInfo, methodInfo.GetParameters()
                .First(parameterInfo => parameterInfo.ParameterType.IsGenericType)
                .ParameterType.GetGenericArguments()))
            .OrderByDescending(x => x.Item2.Length);

        var argumentsCount = callbackArguments.Length + (filterObjectType is null ? 0 :
            callbackArguments.Any(x => x.Name == filterObjectType.Name) ? 0 : 1);

        foreach (var (methodInfo, gameCallbackArguments) in methodsWithCallbackArguments)
        {
            if (gameCallbackArguments.Length < argumentsCount)
                continue;

            if (filterObjectType is not null)
            {
                var firstGameCallbackParameter = gameCallbackArguments.First();
                if (firstGameCallbackParameter.GetUnderlyingName() != filterObjectType.Name)
                    return (null, Array.Empty<Type>());
            }

            if (callbackArguments.All(x =>
                    gameCallbackArguments.Any(y => x.GetUnderlyingName() == y.GetUnderlyingName())))
            {
                return (methodInfo, gameCallbackArguments);
            }
        }

        return (null, Array.Empty<Type>());
    }

    private static void AddSource(Template template, object model, string templateFileName,
        SourceProductionContext context)
    {
        var output = template.Render(model, member => member.Name);
        context.AddSource(templateFileName.Replace("sbn-cs", "g.cs"), SourceText.From(output, Encoding.UTF8));
    }

    private record BuildMethodResult(
        string HookMethodName,
        ImmutableArray<IParameterSymbol> CallbackParameters,
        Type GameCallbackType,
        ITypeSymbol? FilterObjectType,
        ImmutableArray<ITypeSymbol> CallbackArgumentTypes,
        Type[] GameCallbackTypeArguments);
}