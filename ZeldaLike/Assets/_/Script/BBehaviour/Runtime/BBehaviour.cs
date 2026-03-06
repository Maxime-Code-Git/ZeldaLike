using UnityEngine;

namespace BBehaviour.Runtime {
    public enum VerboseType { Log, Warning, Error }

    public class BBehaviour : MonoBehaviour {
        #region Verbose
        public bool isVerbose;
        public bool logs;
        public bool warnings;
        public bool errors;

        public void Verbose(string message, VerboseType type = VerboseType.Log) {
            if(isVerbose) {
                switch(type) {
                    case VerboseType.Log:
                        if(logs) Debug.Log(message);
                        break;
                    case VerboseType.Warning:
                        if(warnings) Debug.LogWarning(message);
                        break;
                    case VerboseType.Error:
                        if(errors) Debug.LogError(message);
                        break;
                }
            }
        }
        #endregion
    }
}