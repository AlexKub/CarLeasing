using System;

namespace RTCManifestGenerator.Commands
{
    public class RelayCommand<T> : SimpleCommand
    {
        private Action<T> _action;

        public RelayCommand(Action<T> action, bool executable = true) : base(executable)
        {
            _action = action;
        }

        public override void Execute(object parameter)
        {
            if (_action != null)

                _action.Invoke((T)parameter);
        }
    }
}
