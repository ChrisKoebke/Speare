using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtimes
{
    public interface IRuntime
    {
        IEnumerator Run(IEnumerator coroutine);

        IEnumerator Say(char[] buffer, int messageIndex, int messageLength);
        IEnumerator SayWithOptions(char[] buffer, int messageIndex, int messageLength, int[] optionIndices, int[] optionLengths);
        IEnumerator ChangeSpeaker(char[] buffer, int speakerIndex, int speakerLength);
        IEnumerator ChangeFace(char[] buffer, int faceIndex, int faceLength);
        IEnumerator RunGameEvent(char[] buffer, int eventIndex, int eventLength);

        object WaitFor(Func<bool> function);
        object WaitForSeconds(float seconds);
        object WaitForEndOfFrame();
        object WaitForFixedUpdate();
    }
}