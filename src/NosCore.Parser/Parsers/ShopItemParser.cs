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
using NosCore.Core;
using NosCore.Core.I18N;
using NosCore.Data.Enumerations.I18N;
using NosCore.Data.StaticEntities;
using NosCore.Database.DAL;
using Serilog;

namespace NosCore.Parser.Parsers
{
    public class ShopItemParser
    {
        private readonly ILogger _logger;
        private readonly IGenericDao<ShopDto> _shopDao;
        private readonly IGenericDao<ShopItemDto> _shopItemDao;

        public ShopItemParser(IGenericDao<ShopItemDto> shopItemDao, IGenericDao<ShopDto> shopDao, ILogger logger)
        {
            _shopItemDao = shopItemDao;
            _shopDao = shopDao;
            _logger = logger;
        }

        public void InsertShopItems(List<string[]> packetList)
        {
            List<ShopItemDto> shopitems = new List<ShopItemDto>();
            int itemCounter = 0;
            byte type = 0;
            foreach (var currentPacket in packetList.Where(o => o[0].Equals("n_inv") || o[0].Equals("shopping")))
            {
                if (currentPacket[0].Equals("n_inv"))
                {
                    short npcid = short.Parse(currentPacket[2]);
                    if (_shopDao.FirstOrDefault(s => s.MapNpcId == npcid) == null)
                    {
                        continue;
                    }

                    for (int i = 5; i < currentPacket.Length; i++)
                    {
                        string[] item = currentPacket[i].Split('.');
                        ShopItemDto sitem = null;

                        if (item.Length == 5)
                        {
                            sitem = new ShopItemDto
                            {
                                ShopId = _shopDao.FirstOrDefault(s => s.MapNpcId == npcid)
                                    .ShopId,
                                Type = type,
                                Slot = byte.Parse(item[1]),
                                ItemVNum = short.Parse(item[2])
                            };
                        }

                        if (item.Length == 6)
                        {
                            sitem = new ShopItemDto
                            {
                                ShopId = _shopDao.FirstOrDefault(s => s.MapNpcId == npcid)
                                    .ShopId,
                                Type = type,
                                Slot = byte.Parse(item[1]),
                                ItemVNum = short.Parse(item[2]),
                                Rare = sbyte.Parse(item[3]),
                                Upgrade = byte.Parse(item[4])
                            };
                        }

                        if (sitem == null || shopitems.Any(s =>
                                s.ItemVNum.Equals(sitem.ItemVNum) && s.ShopId.Equals(sitem.ShopId))
                            || _shopItemDao.Where(s => s.ShopId == sitem.ShopId)
                                .Any(s => s.ItemVNum.Equals(sitem.ItemVNum)))
                        {
                            continue;
                        }

                        shopitems.Add(sitem);
                        itemCounter++;
                    }
                }
                else
                {
                    if (currentPacket.Length > 3)
                    {
                        type = byte.Parse(currentPacket[1]);
                    }
                }
            }

            IEnumerable<ShopItemDto> shopItemDtos = shopitems;

            _shopItemDao.InsertOrUpdate(shopItemDtos);
            _logger.Information(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.SHOPITEMS_PARSED),
                itemCounter);
        }
    }
}