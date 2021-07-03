using System;
using System.Windows.Input;

namespace RatScanner
{
	public class RelayCommand : ICommand
	{
		private Action action;
		private Predicate<object> canExecute;

		public event EventHandler CanExecuteChanged = (sender, e) => { };

		public bool CanExecute(object parameter)
		{
			if (canExecute == null)
			{
				return true;
			}

			return canExecute(parameter);
		}

		public bool IsExecuting { get; set; }

		public RelayCommand(Action action)
		{
			this.action = action;
		}

		public RelayCommand(Action action, Predicate<object> canExecute)
		{
			this.action = action;
			this.canExecute = canExecute;
		}

		public void Execute(object parameter)
		{
			IsExecuting = true;

			action();

			IsExecuting = false;
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
