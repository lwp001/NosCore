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
    [StaticDto(LoadedMessage = LogLanguageKey.MAPS_LOADED, EmptyMessage = LogLanguageKey.NO_MAP)]
    public class MapDto : IStaticDto
    {
        [Key]
        public short MapId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public byte[] Data { get; set; }

        public int Music { get; set; }

        public bool ShopAllowed { get; set; }
    }
}