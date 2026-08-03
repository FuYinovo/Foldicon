using System.IO;
using System.Text.Json;
using Foldicon.Contracts;
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
        var optionData = JsonSerializer.Deserialize<OptionData>(jsonText);
        return optionData ?? new OptionData(); // 读取失败则返回默认值
    }
}