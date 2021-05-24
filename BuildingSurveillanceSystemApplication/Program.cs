using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingSurveillanceSystemApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            SecuritySurveillanceHub securitySurveillanceHub = new SecuritySurveillanceHub();

            EmployeeNotify employeeNotify = new EmployeeNotify(new Employee
            {
                Id = 1,
                FirstName = "Bob",
                LastName = "Jones",
                JobTitle = "Development Manager"
            });
            EmployeeNotify employeeNotify2 = new EmployeeNotify(new Employee
            {
                Id = 2,
                FirstName = "Dave",
                LastName = "Kendal",
                JobTitle = "Chief Information Officer"
            });

            SecurityNotify securityNotify = new SecurityNotify();

            employeeNotify.Subscribe(securitySurveillanceHub);
            employeeNotify2.Subscribe(securitySurveillanceHub);
            securityNotify.Subscribe(securitySurveillanceHub);

            securitySurveillanceHub.ConfirmExternalVisitorEntersBuilding(1, "Andrew", "Jackson", "The Company", "Contractor", DateTime.Parse("12 May 2020 11:00"), 1);
            securitySurveillanceHub.ConfirmExternalVisitorEntersBuilding(2, "Jane", "Davidson", "Another Company", "Lawyer", DateTime.Parse("12 May 2020 12:00"), 2);

           // employeeNotify.UnSubscribe();

            securitySurveillanceHub.ConfirmExternalVisitorExitsBuilding(1, DateTime.Parse("12 May 2020 13:00"));
            securitySurveillanceHub.ConfirmExternalVisitorExitsBuilding(2, DateTime.Parse("12 May 2020 15:00"));

            securitySurveillanceHub.BuildingEntryCutOffTimeReached();

            Console.ReadKey();
        }
    }

    public class Employee : IEmployee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
    }
    public interface IEmployee
    { 
        int Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string JobTitle { get; set; }
    
    }

    public abstract class Observer : IObserver<ExternalVisitor>
    {
        IDisposable _cancellation;
        protected List<ExternalVisitor> _externalVisitors = new List<ExternalVisitor>();

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(ExternalVisitor value);

        public void Subscribe(IObservable<ExternalVisitor> provider)
        {
            _cancellation = provider.Subscribe(this);
        }

        public void UnSubscribe()
        {
            _cancellation.Dispose();
            _externalVisitors.Clear();
        }

    }


    public class EmployeeNotify : Observer
    {
        IEmployee _employee = null;
        public EmployeeNotify(IEmployee employee)
        {
            _employee = employee;
        }
        public override void OnCompleted()
        {
            string heading = $"{_employee.FirstName + " " + _employee.LastName} Daily Visitor's Report";
            Console.WriteLine();

            Console.WriteLine(heading);
            Console.WriteLine(new string('-', heading.Length));
            Console.WriteLine();

            foreach (var externalVisitor in _externalVisitors)
            {
                externalVisitor.InBuilding = false;

                Console.WriteLine($"{externalVisitor.Id,-6}{externalVisitor.FirstName,-15}{externalVisitor.LastName,-15}{externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss"),-25}{externalVisitor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss tt"),-25}");
            }
            Console.WriteLine();
            Console.WriteLine();
        
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(ExternalVisitor value)
        {
            var externalVisitor = value;

            if (externalVisitor.EmployeeContactId == _employee.Id)
            {
                var externalVisitorListItem = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitor.Id);

                if (externalVisitorListItem == null)
                {
                    _externalVisitors.Add(externalVisitor);
                    
                    OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Employee);
                    
                    Console.WriteLine($"{_employee.FirstName + " " + _employee.LastName}, your visitor has arrived. Visitor ID({externalVisitor.Id}), FirstName({externalVisitor.FirstName}), LastName({externalVisitor.LastName}), entered the building, DateTime({externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss")})");
                    
                    OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Normal);
                    
                    Console.WriteLine();
                }
                else
                {
                    if (externalVisitor.InBuilding == false)
                    {
                        //update local external visitor list item with data from the external visitor object passed in from the observable object
                        externalVisitorListItem.InBuilding = false;
                        externalVisitorListItem.ExitDateTime = externalVisitor.ExitDateTime;
                    }
                }

            }
        }

    }

    public class UnSubscriber<ExternalVisitor> : IDisposable
    {
        private List<IObserver<ExternalVisitor>> _observers;
        private IObserver<ExternalVisitor> _observer;
        public UnSubscriber(List<IObserver<ExternalVisitor>> observers, IObserver<ExternalVisitor> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
    public class SecurityNotify : Observer
    {

        public override void OnCompleted()
        {
            string heading = "Security Daily Visitor's Report";
            Console.WriteLine();

            Console.WriteLine(heading);
            Console.WriteLine(new string('-', heading.Length));
            Console.WriteLine();

            foreach (var externalVisitor in _externalVisitors)
            {
                externalVisitor.InBuilding = false;

                Console.WriteLine($"{externalVisitor.Id,-6}{externalVisitor.FirstName,-15}{externalVisitor.LastName,-15}{externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss"),-25}{externalVisitor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss tt"),-25}");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(ExternalVisitor value)
        {
            var externalVisitor = value;

            var externalVisitorListItem = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitor.Id);

            if (externalVisitorListItem == null)
            {
                _externalVisitors.Add(externalVisitor);
                
                OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Security);

                Console.WriteLine($"Security notification: Visitor Id({externalVisitor.Id}), FirstName({externalVisitor.FirstName}), LastName({externalVisitor.LastName}), entered the building, DateTime({externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss tt")})");

                OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Normal);

                Console.WriteLine();
            }
            else
            {
                if (externalVisitor.InBuilding == false)
                {
                     //update local external visitor list item with data from the external visitor object passed in from the observable object
                    externalVisitorListItem.InBuilding = false; 
                    externalVisitorListItem.ExitDateTime = externalVisitor.ExitDateTime;
                    
                    Console.WriteLine($"Security notification: Visitor Id({externalVisitor.Id}), FirstName({externalVisitor.FirstName}), LastName({externalVisitor.LastName}), exited the building, DateTime({externalVisitor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss tt")})");
                    Console.WriteLine();

                }
            }

        }
    }




    public class SecuritySurveillanceHub : IObservable<ExternalVisitor>
    {
        private List<ExternalVisitor> _externalVisitors;
        private List<IObserver<ExternalVisitor>> _observers;

        public SecuritySurveillanceHub()
        {
            _externalVisitors = new List<ExternalVisitor>();
            _observers = new List<IObserver<ExternalVisitor>>();
        }

        public IDisposable Subscribe(IObserver<ExternalVisitor> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);

            foreach (var externalVisitor in _externalVisitors)
                observer.OnNext(externalVisitor);

            return new UnSubscriber<ExternalVisitor>(_observers, observer);

        }

        public void ConfirmExternalVisitorEntersBuilding(int id, string firstName, string lastName, string companyName, string jobTitle, DateTime entryDateTime, int employeeContactId)
        {
            ExternalVisitor externalVisitor = new ExternalVisitor
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                CompanyName = companyName,
                JobTitle = jobTitle,
                EntryDateTime = entryDateTime,
                InBuilding = true,
                EmployeeContactId = employeeContactId
            };

            _externalVisitors.Add(externalVisitor);

            foreach (var observer in _observers)
                observer.OnNext(externalVisitor);

        }
        public void ConfirmExternalVisitorExitsBuilding(int externalVisitorId, DateTime exitDateTime)
        {
            var externalVisitor = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitorId);

            if (externalVisitor != null)
            {
                externalVisitor.ExitDateTime = exitDateTime;
                externalVisitor.InBuilding = false;

                foreach (var observer in _observers)
                    observer.OnNext(externalVisitor);
            }
        }
        public void BuildingEntryCutOffTimeReached()
        {
            if (_externalVisitors.Any(e => e.InBuilding == true))
            {
                return;
            }

            foreach (var observer in _observers)
                observer.OnCompleted();
        }
    }
    public static class OutputFormatter
    {
        public enum TextOutputTheme
        { 
            Security,
            Employee,
            Normal
        }

        public static void ChangeOutputTheme(TextOutputTheme textOutputTheme)
        {
            if (textOutputTheme == TextOutputTheme.Employee)
            {
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (textOutputTheme == TextOutputTheme.Security)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else
            {
                Console.ResetColor();
            }
        
        }

    }

    public class ExternalVisitor
    { 
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public DateTime EntryDateTime { get; set; }
        public DateTime ExitDateTime { get; set; }
        public bool InBuilding { get; set; }
        public int EmployeeContactId { get; set; }

    }

}
