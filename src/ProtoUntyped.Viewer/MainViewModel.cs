using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Kernel;
using PropertyChanged;
using ProtoUntyped.Viewer.Messages;

namespace ProtoUntyped.Viewer
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        public MainViewModel()
        {
            Messages = GenerateMessages();
            Options.PropertyChanged += (_, _) => ComputeProtoObject();
        }

        public DecodeOptionsViewModel Options { get; } = new();
        public List<MessageViewModel> Messages { get; }
        public MessageViewModel SelectedMessage { get; set; }
        
        public ProtoObject ProtoObject { get; private set; }
        
        public string ProtoObjectJson => ProtoObject != null ? JsonSerializer.Serialize(ProtoObject.ToFieldDictionary(), new JsonSerializerOptions { WriteIndented = true }) : null;
        public string ProtoObjectString => ProtoObject?.ToString();

        private static List<MessageViewModel> GenerateMessages()
        {
            var fixture = new Fixture();
            var types = typeof(MainViewModel).Assembly
                                             .GetTypes()
                                             .Where(x => x.Namespace == typeof(InstanceStarted).Namespace && x.IsClass && !x.IsNested)
                                             .ToList();

            var messages = types.SelectMany(x => Enumerable.Range(0, 10).Select(_ => CreateMessage(x)));
            
            return messages.Select(x => new MessageViewModel(x)).ToList();

            object CreateMessage(Type type)
            {
                var context = new SpecimenContext(fixture);
                return context.Resolve(new SeededRequest(type, null));
            }
        }

        private void OnSelectedMessageChanged()
        {
            ComputeProtoObject();
        }

        private void ComputeProtoObject()
        {
            ProtoObject = SelectedMessage != null ? ProtoObject.Decode(SelectedMessage.ProtoBytes, Options.ProtoDecodeOptions) : null;
        }
    }
}
