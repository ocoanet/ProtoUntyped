using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoFixture;
using AutoFixture.Kernel;
using ProtoUntyped.Viewer.Annotations;
using ProtoUntyped.Viewer.Messages;

namespace ProtoUntyped.Viewer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MessageViewModel _selectedMessage;

        public MainViewModel()
        {
            Messages = GenerateMessages();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public List<MessageViewModel> Messages { get; set; }

        public MessageViewModel SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                if (Equals(value, _selectedMessage))
                    return;
                _selectedMessage = value;
                OnPropertyChanged();
            }
        }

        private static List<MessageViewModel> GenerateMessages()
        {
            var fixture = new Fixture();
            var types = typeof(MainViewModel).Assembly
                                             .GetTypes()
                                             .Where(x => x.Namespace == typeof(InstanceStarted).Namespace)
                                             .ToList();

            var messages = types.SelectMany(x => Enumerable.Range(0, 10).Select(_ => CreateMessage(x)));
            
            return messages.Select(x => new MessageViewModel(x)).ToList();

            object CreateMessage(Type type)
            {
                var context = new SpecimenContext(fixture);
                return context.Resolve(new SeededRequest(type, null));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
