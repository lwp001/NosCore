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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using NosCore.Core;
using NosCore.Core.I18N;
using NosCore.Data.AliveEntities;
using NosCore.Data.Enumerations.I18N;
using NosCore.Data.StaticEntities;
using NosCore.Database.DAL;
using Serilog;

namespace NosCore.Parser.Parsers
{
    public class ShopParser
    {
        private readonly ILogger _logger;
        private readonly IGenericDao<ShopDto> _shopDao;
        private readonly IGenericDao<MapNpcDto> _mapNpcDao;
        public ShopParser(IGenericDao<ShopDto> shopDao, IGenericDao<MapNpcDto> mapNpcDao, ILogger logger)
        {
            _shopDao = shopDao;
            _mapNpcDao = mapNpcDao;
            _logger = logger;
        }

        public void InsertShops(List<string[]> packetList)
        {
            int shopCounter = 0;
            var shops = new List<ShopDto>();
            foreach (var currentPacket in packetList.Where(o => o.Length > 6 && o[0].Equals("shop") && o[1].Equals("2"))
            )
            {
                short npcid = short.Parse(currentPacket[2]);
                var npc = _mapNpcDao.FirstOrDefault(s => s.MapNpcId == npcid);
                if (npc == null)
                {
                    continue;
                }

                StringBuilder name = new StringBuilder();
                for (int j = 6; j < currentPacket.Length; j++)
                {
                    name.Append($"{currentPacket[j]}");
                    if (j != currentPacket.Length - 1)
                    {
                        name.Append(" ");
                    }
                }

                var shop = new ShopDto
                {
                    Name = name.ToString(),
                    MapNpcId = npc.MapNpcId,
                    MenuType = byte.Parse(currentPacket[4]),
                    ShopType = byte.Parse(currentPacket[5])
                };

                if (_shopDao.FirstOrDefault(s => s.MapNpcId == npc.MapNpcId) != null ||
                    shops.Any(s => s.MapNpcId == npc.MapNpcId))
                {
                    continue;
                }

                shops.Add(shop);
                shopCounter++;
            }

            IEnumerable<ShopDto> shopDtos = shops;
            _shopDao.InsertOrUpdate(shopDtos);

            _logger.Information(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.SHOPS_PARSED),
                shopCounter);
        }
    }
}