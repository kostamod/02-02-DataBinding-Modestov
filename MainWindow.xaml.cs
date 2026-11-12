using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ServiceApp;

public partial class MainWindow : Window
{
    private readonly string folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WpfService10");
    private Service Current => (Service)DataContext;
    private bool saved;

    public MainWindow()
    {
        InitializeComponent();
        NewRecord();
    }

    private string RecordPath(int id) => Path.Combine(folder, $"{id}.json");

    private int NewId()
    {
        int id;
        do { id = Random.Shared.Next(1, int.MaxValue); }
        while (File.Exists(RecordPath(id)));
        return id;
    }

    private void NewRecord()
    {
        DataContext = new Service { Id = NewId() };
        saved = false;
        Status.Text = "Новая запись.";
    }

    private void New_Click(object sender, RoutedEventArgs e) => NewRecord();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (Validation.GetHasError(RateBox) || Validation.GetHasError(HoursBox) ||
            string.IsNullOrWhiteSpace(Current.Name) || string.IsNullOrWhiteSpace(Current.Worker) ||
            Current.Hours == 0)
        {
            Status.Text = "Заполните все поля. Часы должны быть больше нуля.";
            return;
        }

        try
        {
            Directory.CreateDirectory(folder);
            Service service = Current;
            while (true)
            {
                string path = RecordPath(service.Id);
                string temporary = Path.Combine(folder, $"{Guid.NewGuid()}.tmp");
                File.WriteAllText(temporary, JsonSerializer.Serialize(service, new JsonSerializerOptions { WriteIndented = true }));
                try
                {
                    File.Move(temporary, path, overwrite: saved);
                    saved = true;
                    Status.Text = $"Запись {service.Id} сохранена.";
                    break;
                }
                catch (IOException) when (!saved && File.Exists(path))
                {
                    service.Id = NewId();
                }
                finally
                {
                    File.Delete(temporary);
                }
            }
        }
        catch (IOException) { Status.Text = "Не удалось сохранить запись."; }
        catch (UnauthorizedAccessException) { Status.Text = "Нет доступа к папке данных."; }
    }

    private void Find_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(Search.Text, out int id) || id <= 0)
        {
            Status.Text = "Введите положительный целый ID.";
            return;
        }

        string path = RecordPath(id);
        if (!File.Exists(path))
        {
            Status.Text = "Запись не найдена.";
            return;
        }
        try
        {
            Service? service = JsonSerializer.Deserialize<Service>(File.ReadAllText(path));
            if (service is null || service.Id != id) throw new JsonException();
            DataContext = service;
            saved = true;
            Status.Text = "Запись найдена.";
        }
        catch (JsonException) { Status.Text = "Файл записи повреждён."; }
        catch (ArgumentOutOfRangeException) { Status.Text = "Цена или часы в файле выходят за допустимые пределы."; }
        catch (IOException) { Status.Text = "Не удалось открыть запись."; }
        catch (UnauthorizedAccessException) { Status.Text = "Нет доступа к файлу."; }
    }
}
