using System.Collections.Generic;
using System.Collections.ObjectModel;
using Foldicon.Models;
using Foldicon.Struct;

namespace Foldicon.Contracts;

public interface IIconGroupService
{
     ObservableCollection<IconGroup> Groups { get; }
     void Add(string name, string description, IconGroupLogo logo, List<IFolderIcon> icons);
     void Remove(IconGroup group);
     void Edit(IconGroup group, string name, string description, IconGroupLogo logo);
}