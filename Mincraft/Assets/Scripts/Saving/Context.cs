using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Context<T>
{
    /// <summary>
    /// Returns null in case of not overwriting
    /// </summary>
    /// <returns></returns>
    public virtual object Data()
    {
        return null;
    }

    public virtual T Caster(object data)
    {
        return default(T);
    }
}
