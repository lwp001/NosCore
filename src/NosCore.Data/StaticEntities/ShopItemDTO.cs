﻿//  __  _  __    __   ___ __  ___ ___  
// |  \| |/__\ /' _/ / _//__\| _ \ __| 
// | | ' | \/ |`._`.| \_| \/ | v / _|  
// |_|\__|\__/ |___/ \__/\__/|_|_\___| 
// 
// Copyright (C) 2019 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations;
using NosCore.Data.DataAttributes;
using LogLanguageKey = NosCore.Data.Enumerations.I18N.LogLanguageKey;

namespace NosCore.Data.StaticEntities
{
    [StaticDto(LoadedMessage = LogLanguageKey.SHOPITEMS_LOADED)]
    public class ShopItemDto : IStaticDto
    {
        public byte Color { get; set; }

        public short ItemVNum { get; set; }

        public short Rare { get; set; }

        public int ShopId { get; set; }

        [Key]
        public int ShopItemId { get; set; }

        public byte Slot { get; set; }

        public byte Type { get; set; }

        public byte Upgrade { get; set; }
    }
}