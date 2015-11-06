using UnityEngine;
using System.Collections;

[SerializeField]
public class SObject
{
    public string Name;
    public GameObject Obj;

    public SObject(string name,GameObject obj)
    {
        Name = name;
        Obj = obj;
    }
}
