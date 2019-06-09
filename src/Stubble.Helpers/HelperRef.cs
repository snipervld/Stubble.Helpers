﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stubble.Helpers
{
    public struct HelperRef : IEquatable<HelperRef>
    {
        public HelperRef(Delegate @delegate)
        {
            Delegate = @delegate;

            var @params = @delegate.Method.GetParameters();
            var builder = ImmutableArray.CreateBuilder<Type>(@params.Length);
            foreach (var arg in @params)
            {
                builder.Add(arg.ParameterType);
            }
            ArgumentTypes = builder.ToImmutable();
        }

        public Delegate Delegate;
        public ImmutableArray<Type> ArgumentTypes;

        public override bool Equals(object obj)
        {
            return obj is HelperRef @ref && Equals(@ref);
        }

        public bool Equals(HelperRef other)
        {
            return EqualityComparer<Delegate>.Default.Equals(Delegate, other.Delegate) &&
                   ArgumentTypes.Equals(other.ArgumentTypes);
        }

        public override int GetHashCode()
        {
            var hashCode = -1973005441;
            hashCode = hashCode * -1521134295 + EqualityComparer<Delegate>.Default.GetHashCode(Delegate);
            foreach (var type in ArgumentTypes)
            {
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            }
            return hashCode;
        }
    }
}
