using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Contracts;
using Foldicon.Messages;
using Foldicon.Struct;

namespace Foldicon.Services;

public class JsonOptionService(string jsonPath) : IOptionService
{
    public OptionData Options { get; set; } = LoadOptions(jsonPath);

    public void SaveAll()
    {
        var jsonText = JsonSerializer.Serialize(Options);
        File.WriteAllText(jsonPath, jsonText);
    }

    private static OptionData LoadOptions(string jsonPath)
    {
        if (!File.Exists(jsonPath)) return new OptionData();

        var jsonText = File.ReadAllText(jsonPath);
        var optionData = JsonSerializer.Deserialize<OptionData>(jsonText) ?? new OptionData();
        ApplyRuntimeSettings(optionData);
        return optionData;
    }

    private static void ApplyRuntimeSettings(OptionData optionData)
    {
        WeakReferenceMessenger.Default.Send(new AppThemeChangedMessage { NewTheme = optionData.AppTheme });
        WeakReferenceMessenger.Default.Send(new AppBackdropChangedMessage { NewBackdrop = optionData.AppBackdrop });
    }
}