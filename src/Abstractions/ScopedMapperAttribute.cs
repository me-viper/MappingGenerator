// <copyright file="ScopedMapperAttribute.cs" company="ZoralLabs">
//   Copyright Zoral Limited 2017 all rights reserved.
//   Copyright Zoral Inc. 2017 all rights reserved.
// </copyright>

using System;

namespace Talk2Bits.MappingGenerator.Abstractions
{
    public abstract class ScopedMapperAttribute : Attribute
    {
        public string? AppliesTo { get; set; }
    }
}