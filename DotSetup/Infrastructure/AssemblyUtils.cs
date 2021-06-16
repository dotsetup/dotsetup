// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotSetup.Infrastructure
{
    public static class AssemblyUtils
    {
        public static T GetAssemblyAttribute<T>(this Assembly ass) where T : Attribute
        {
            object[] attributes = ass.GetCustomAttributes(typeof(T), false);
            if (attributes == null || attributes.Length == 0)
                return null;
            return attributes.OfType<T>().SingleOrDefault();
        }

        public static string GetFileVersion(Assembly assembly)
        {
            AssemblyFileVersionAttribute attribute = assembly.GetAssemblyAttribute<AssemblyFileVersionAttribute>();
            if (attribute == null)
                return string.Empty;
            return attribute.Version;
        }
    }    
}
