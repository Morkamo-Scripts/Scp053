using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Interfaces;
using Scp053.Variants;

namespace Scp053
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public Scp053ClassD Scp053ClassD { get; set; } = new();
        public Scp053Chaos Scp053Chaos { get; set; } = new();
        public Scp053Ntf Scp053Ntf { get; set; } = new();
        
        public HashSet<ItemType> NotAllowedItems { get; set; } =
        [
            ItemType.SCP207
        ];
    }
}