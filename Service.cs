using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ServiceApp;

public class Service : INotifyPropertyChanged
{
    private decimal rate;
    private decimal hours;
    private int id;
    public int Id
    {
        get => id;
        set
        {
            id = value;
            PropertyChanged?.Invoke(this, new(nameof(Id)));
        }
    }
    public string Name { get; set; } = "";
    public string Worker { get; set; } = "";
    public decimal Rate
    {
        get => rate;
        set
        {
            if (value < 0 || value > 1_000_000_000) throw new ArgumentOutOfRangeException();
            rate = value;
            Changed(nameof(Rate));
        }
    }
    public decimal Hours
    {
        get => hours;
        set
        {
            if (value < 0 || value > 1_000_000_000) throw new ArgumentOutOfRangeException();
            hours = value;
            Changed(nameof(Hours));
        }
    }
    [JsonIgnore]
    public decimal Total => Rate * Hours;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Changed(string property)
    {
        PropertyChanged?.Invoke(this, new(property));
        PropertyChanged?.Invoke(this, new(nameof(Total)));
    }
}
