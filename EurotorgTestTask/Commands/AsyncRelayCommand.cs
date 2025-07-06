namespace EurotorgTestTask.Commands
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<Task> _execute;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        // ReSharper disable once AsyncVoidMethod
        public async void Execute(object parameter) => await _execute();
    }
}