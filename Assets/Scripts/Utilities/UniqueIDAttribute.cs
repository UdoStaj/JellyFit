using System;
using UnityEngine;
/// <summary>
/// Unique attribute. sinifa ozeldir ve ID icin tasarlanmistir. editor zamaninda her sinifa ozel uniq ID degeri olusturmaya yarar.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class UniqueAttribute : PropertyAttribute
{
    // Marker attribute
}