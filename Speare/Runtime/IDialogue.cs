using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtime
{
    public interface IDialogue
    {
        IEnumerator Say(string message);
        IEnumerator SayWithOptions(string message, string[] options);
        IEnumerator ChangeSpeaker(string speakerName);
        IEnumerator ChangeFace(string face);
        IEnumerator RunEvent(string eventName);
    }
}
