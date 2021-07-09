using System;
using System.Windows.Input;

namespace RatScanner
{
	public class RelayCommand : ICommand
	{
		private readonly Action action;
		private readonly Predicate<object> canExecute;
		private bool isExecuting;

		public event EventHandler CanExecuteChanged = (sender, e) => { };

		public bool CanExecute(object parameter)
		{
			if (IsExecuting)
			{
				return false;
			}

			if (canExecute == null)
			{
				return true;
			}

			return canExecute(parameter);
		}

		public bool IsExecuting
		{
			get => isExecuting;
			set
			{
				isExecuting = value;
				RaiseCanExecuteChanged();
			}
		}

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
