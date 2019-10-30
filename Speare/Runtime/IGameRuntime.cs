using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtime
{
    public interface IGameRuntime
    {
        IEnumerator Say(string message);
        IEnumerator SayWithOptions(string message, string[] options);
        IEnumerator ChangeSpeaker(string speakerName);
        IEnumerator ChangeFace(string faceName);
        IEnumerator RunGameEvent(string gameEvent);

        object WaitFor(Func<float, bool> function);
        object WaitForSeconds(float seconds);
        object WaitForEndOfFrame();
        object WaitForFixedUpdate();
    }
}