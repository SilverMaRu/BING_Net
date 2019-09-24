using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class TypeHelper
{
    private static Type[] allTypes;
    private static Type[] AllTypes
    {
        get
        {
            if(allTypes == null)
            {
                Init();
            }
            return allTypes;
        }
    }

    public static void Init()
    {
        allTypes = Assembly.GetExecutingAssembly().GetExportedTypes();
    }

    public static Type GetType(string typeName)
    {
        return GetType(AllTypes, typeName);
    }

    public static Type GetType(Type[] sourceTypeArray, string typeName)
    {
        Type resultType = null;
        foreach (Type tempType in sourceTypeArray)
        {
            if (tempType.Name.Equals(typeName))
            {
                resultType = tempType;
                break;
            }
        }
        return resultType;
    }

    public static Type[] GetSubclassesOf(Type baseClass)
    {
        List<Type> resultList = new List<Type>();
        foreach(Type tempType in AllTypes)
        {
            if (tempType.IsSubclassOf(baseClass))
            {
                resultList.Add(tempType);
            }
        }
        return resultList.ToArray();
    }
}
