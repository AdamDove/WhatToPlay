using System;
using System.Windows.Input;

namespace WhatToPlay.Common
{
    public class CommandDelegate : ICommand
    {
        private Action m_action;
        private bool m_canExecute;

        public event EventHandler CanExecuteChanged;

        public CommandDelegate(Action action, bool canExecute)
        {
            m_action = action;
            m_canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return m_canExecute;
        }

        public void Execute(object parameter)
        {
            m_action();
        }
    }
}
