using System;

namespace LuckyBlocks.Reflection;

[AttributeUsage(AttributeTargets.Class)]
internal class InjectAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
internal class InjectGameAttribute : InjectAttribute
{
}

[AttributeUsage(AttributeTargets.Property)]
internal class InjectLoggerAttribute : InjectAttribute
{
}

[AttributeUsage(AttributeTargets.Property)]
internal class InjectWeaponsMapperAttribute : InjectAttribute
{
}

[AttributeUsage(AttributeTargets.Property)]
internal class InjectTimeProviderAttribute : InjectAttribute
{
}

[AttributeUsage(AttributeTargets.Property)]
internal class InjectMappedObjectsServiceAttribute : InjectAttribute
{
}