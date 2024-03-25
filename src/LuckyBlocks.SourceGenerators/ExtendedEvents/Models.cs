using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace LuckyBlocks.SourceGenerators.ExtendedEvents;

internal record ExtendedEventsModel(string Namespace, string ClassName,
    ImmutableArray<CallbacksStorageModel> CallbacksStorageModels, ImmutableArray<GameCallbackModel> GameCallbackModels,
    ImmutableArray<HookMethodModel> HookMethodModels, ImmutableArray<GameCallbackMethodModel> GameCallbackMethodModels);

internal record CallbacksStorageModel(string Name, string Type, Type GameCallbackType,
    ImmutableArray<ITypeSymbol> ArgumentTypes, string CallbackTypeArguments, string CallbackType,
    bool FilterObjectExists, Type[] GameCallbackArgumentTypes)
{
    public string EventCtorParametersAsString => GetEventCtorParametersAsString();
    public string QueueItemArgsAsString => GetQueueItemArgsAsString();

    private string GetEventCtorParametersAsString()
    {
        return ArgumentTypes switch
        {
            [] => "handled",
            [_] =>
                $"arg{GameCallbackArgumentTypes.ToList().FindIndex(
                    x => x.GetUnderlyingName() == ArgumentTypes[0].GetUnderlyingName()) + 1}, handled",
            [_, _] =>
                $"arg{GameCallbackArgumentTypes.ToList().FindIndex(
                    x => x.GetUnderlyingName() == ArgumentTypes[0].GetUnderlyingName()) + 1}, arg{
                    GameCallbackArgumentTypes.ToList().FindIndex(x => x.GetUnderlyingName() == ArgumentTypes[1].GetUnderlyingName()) + 1}, handled"
        };
    }
    
    private string GetQueueItemArgsAsString()
    {
        return ArgumentTypes switch
        {
            [] => string.Empty,
            [_] =>
                $", arg{GameCallbackArgumentTypes.ToList().FindIndex(
                    x => x.GetUnderlyingName() == ArgumentTypes[0].GetUnderlyingName()) + 1}",
            [_, _] =>
                $", arg{GameCallbackArgumentTypes.ToList().FindIndex(
                    x => x.GetUnderlyingName() == ArgumentTypes[0].GetUnderlyingName()) + 1}, arg{
                    GameCallbackArgumentTypes.ToList().FindIndex(x => x.GetUnderlyingName() == ArgumentTypes[1].GetUnderlyingName()) + 1}"
        };
    }
}

internal record GameCallbackModel(string Name, string Type, string CallbackMethodName,
    ImmutableArray<Type> ArgumentTypes, bool FirstArgumentIsArray,
    ImmutableArray<CallbacksStorageModel> LinkedCallbacks);

internal record HookMethodModel(HookMethodSignatureModel Signature, HookMethodBodyModel Body);

internal record HookMethodSignatureModel(string Name, ImmutableArray<IParameterSymbol> Parameters)
{
    public string ParametersAsString => GetParametersAsString();

    public string GetParametersAsString()
    {
        var sb = new StringBuilder();
        var filterObjectExists = !Parameters.First().IsDelegate();
        var parameters = (IEnumerable<IParameterSymbol>)Parameters;

        if (filterObjectExists)
        {
            foreach (var part in parameters.First().ToDisplayParts()[..^1])
            {
                sb.Append(part);
            }

            sb.Append("filterObject, ");

            parameters = parameters.Skip(1);
        }

        var parameterNames = new List<string> { "callbackDelegate", "hookMode", "ignoreHandled" };
        var parameterIndex = 0;
        foreach (var parameter in parameters)
        {
            var parts = parameter.ToDisplayParts()[..^1];
            foreach (var part in parts)
            {
                sb.Append(part);
            }

            sb.Append(parameterNames[parameterIndex]);

            if (parameter is { IsOptional: true, HasExplicitDefaultValue: true })
            {
                sb.Append(" = ");
                sb.Append(parameter.ExplicitDefaultValue!.ToString().ToLower());
            }

            parameterIndex++;

            sb.Append(", ");
        }

        sb.Remove(sb.Length - ", ".Length, ", ".Length);

        return sb.ToString();
    }
}

internal record HookMethodBodyModel(string CallbackCtorParameters, CallbacksStorageModel CallbacksStorage)
{
    public string CallbackType => CallbacksStorage.CallbackType;
    public bool FilterObjectExists => CallbacksStorage.FilterObjectExists;
}

internal record GameCallbackMethodModel(GameCallbackMethodSignatureModel Signature,
    GameCallbackMethodBodyModel Body);

internal record GameCallbackMethodSignatureModel(GameCallbackModel GameCallback)
{
    public string Name => GameCallback.CallbackMethodName;
    public string ParametersAsString => GetParametersAsString();

    private ImmutableArray<Type> ArgumentTypes => GameCallback.ArgumentTypes;

    private string GetParametersAsString()
    {
        var parameterId = 1;
        return string.Join(", ", ArgumentTypes.Select(x => $"{x.Name} arg{parameterId++}"));
    }
}

internal record GameCallbackMethodBodyModel(GameCallbackModel GameCallback)
{
    public ImmutableArray<CallbacksStorageModel> LinkedCallbacks => GameCallback.LinkedCallbacks;
    public bool FirstArgumentIsArray => GameCallback.FirstArgumentIsArray;
}