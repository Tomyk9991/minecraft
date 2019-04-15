using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldContext<T> where T : Context
{
    //Als Kontext kann man z. B. Chunks betrachten
    //Als Kontext kann man auch den Serializer und Deserializer betrachten, denn der wird immer of type T zurückgeben
}

public class Context
{

}
