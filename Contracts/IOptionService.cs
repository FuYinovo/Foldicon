using Foldicon.Struct;

namespace Foldicon.Contracts;

public interface IOptionService
{
    OptionData Options { get; set;}
    void SaveAll();
}