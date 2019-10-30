using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtime
{
    public delegate bool WaitForDelegate(float dt);

    public interface ICoroutineRuntime
    {
        object WaitFor(WaitForDelegate function);
        object WaitForSeconds(float seconds);
        object WaitForEndOfFrame();
        object WaitForFixedUpdate();
    }
}
