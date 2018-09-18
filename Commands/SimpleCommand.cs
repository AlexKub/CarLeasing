using System;
using System.Windows.Input;

namespace RTCManifestGenerator.Commands
{
    /// <summary>
    /// Простая реализация ICommand
    /// </summary>
    public abstract class SimpleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private bool _canExecute;

        public SimpleCommand(bool canExecute)
        {
            _canExecute = canExecute;
        }

        public bool Executable
        {
            get { return CanExecute(null); }
            set
            {
                if (value != _canExecute)
                    CanExecuteChanged(this, EventArgs.Empty);

                _canExecute = value;
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public abstract void Execute(object parameter);
    }
}