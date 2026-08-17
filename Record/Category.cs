using System.Collections.Generic;

namespace Foldicon.Record;

public record Category<T>
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<T> Items { get; set; } = [];

    public override string ToString() => Name;
}