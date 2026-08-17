using System;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using Foldicon.Contracts;
using Foldicon.Enums;
using Foldicon.Messages;
using Foldicon.Record;

namespace Foldicon.Services;

public class JsonOptionService(IUserDataService userDataService) : IOptionService
{
    public OptionData Options { get; set; } = LoadOptions(userDataService.GetPath(UserData.SettingsFile));

    public void SaveAll()
    {
        var jsonText = JsonSerializer.Serialize(Options);
        File.WriteAllText(userDataService.GetPath(UserData.SettingsFile), jsonText);
    }

    private static OptionData LoadOptions(string jsonPath)
    {
        if (!File.Exists(jsonPath)) return new OptionData();

        var jsonText = File.ReadAllText(jsonPath);

        // 防止空文件直接在 Deserialize<T>() 抛出异常
        OptionData optionData;
        try
        {
            optionData = JsonSerializer.Deserialize<OptionData>(jsonText) ?? new OptionData();
        }
        catch (JsonException)
        {
            optionData = new OptionData();
        }

        ApplyRuntimeSettings(optionData);
        return optionData;
    }

    private static void ApplyRuntimeSettings(OptionData optionData)
    {
        WeakReferenceMessenger.Default.Send(new AppThemeChangedMessage { NewTheme = optionData.AppTheme });
        WeakReferenceMessenger.Default.Send(new AppBackdropChangedMessage { NewBackdrop = optionData.AppBackdrop });
    }
}