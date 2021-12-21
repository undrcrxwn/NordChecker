using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NordChecker.Infrastructure.Commands
{
    public class DelegateCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }

        private List<INotifyPropertyChanged> propertiesToListenTo;
        private List<WeakReference> ControlEvent;

        public DelegateCommand()
        {
            ControlEvent = new List<WeakReference>();
        }

        public List<INotifyPropertyChanged> PropertiesToListenTo
        {
            get { return propertiesToListenTo; }
            set
            {
                propertiesToListenTo = value;
            }
        }

        private Action<object> executeDelegate;

        public Action<object> ExecuteDelegate
        {
            get { return executeDelegate; }
            set
            {
                executeDelegate = value;
                ListenForNotificationFrom((INotifyPropertyChanged)executeDelegate.Target);
            }
        }

        public static ICommand Create(Action<object> exec)
        {
            return new SimpleCommand { ExecuteDelegate = exec };
        }



        #region ICommand Members


        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true; // if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                ControlEvent.Add(new WeakReference(value));
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                ControlEvent.Remove(ControlEvent.Find(r => ((EventHandler)r.Target) == value));
            }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }
        #endregion

        public void RaiseCanExecuteChanged()
        {
            if (ControlEvent != null && ControlEvent.Count > 0)
            {
                ControlEvent.ForEach(ce =>
                {
                    if (ce.Target != null)
                        ((EventHandler)(ce.Target)).Invoke(null, EventArgs.Empty);
                });
            }
        }



        public DelegateCommand ListenOn<TObservedType, TPropertyType>(TObservedType viewModel, Expression<Func<TObservedType, TPropertyType>> propertyExpression) where TObservedType : INotifyPropertyChanged
        {
            string propertyName = GetPropertyName(propertyExpression);
            viewModel.PropertyChanged += (PropertyChangedEventHandler)((sender, e) =>
            {
                if (e.PropertyName == propertyName) RaiseCanExecuteChanged();
            });
            return this;
        }

        public void ListenForNotificationFrom<TObservedType>(TObservedType viewModel) where TObservedType : INotifyPropertyChanged
        {
            viewModel.PropertyChanged += (PropertyChangedEventHandler)((sender, e) =>
            {
                RaiseCanExecuteChanged();
            });
        }

        private string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression) where T : INotifyPropertyChanged
        {
            var lambda = expression as LambdaExpression;
            MemberInfo memberInfo = GetmemberExpression(lambda).Member;
            return memberInfo.Name;
        }

        private MemberExpression GetmemberExpression(LambdaExpression lambda)
        {
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
                memberExpression = lambda.Body as MemberExpression;
            return memberExpression;
        }

    }
}
