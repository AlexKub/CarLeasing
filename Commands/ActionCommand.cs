using System;

namespace RTCManifestGenerator.Commands
{
    /// <summary>
    /// Комманда без параметров
    /// </summary>
    public sealed class ActionCommand : SimpleCommand
    {
        private Action _action;

        public ActionCommand(Action action, bool executable = true) : base(executable)
        {
            _action = action;
        }

        public override void Execute(object parameter)
        {
            if (_action != null)

                _action.Invoke();
        }
    }
}