using System.ComponentModel;
using PropertyChanged;

#pragma warning disable 67

namespace ProtoUntyped.Viewer;

[AddINotifyPropertyChangedInterface]
public class DecodeOptionsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool DecodeGuid { get; set; } = true;
    public bool DecodeDateTime { get; set; } = true;
    public bool DecodeTimeSpan { get; set; } = true;
    public bool DecodeDecimal { get; set; } = true;

    public ProtoDecodeOptions ProtoDecodeOptions => new()
    {
        DecodeGuid = DecodeGuid,
        DecodeDateTime = DecodeDateTime,
        DecodeTimeSpan = DecodeTimeSpan,
        DecodeDecimal = DecodeDecimal,
    };
}