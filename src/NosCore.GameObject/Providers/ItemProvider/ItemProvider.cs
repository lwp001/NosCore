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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Mapster;
using NosCore.Data;
using NosCore.Data.Enumerations.Items;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.GameObject.Providers.ItemProvider.Item;
using NosCore.Packets.ClientPackets;

namespace NosCore.GameObject.Providers.ItemProvider
{
    public class ItemProvider : IItemProvider
    {
        private readonly List<IHandler<Item.Item, Tuple<IItemInstance, UseItemPacket>>> _handlers;
        private readonly List<ItemDto> _items;

        public ItemProvider(List<ItemDto> items,
            IEnumerable<IHandler<Item.Item, Tuple<IItemInstance, UseItemPacket>>> handlers)
        {
            _items = items;
            _handlers = handlers.ToList();
        }

        public IItemInstance Convert(IItemInstanceDto k)
        {
            IItemInstance item =
                k.Adapt<BoxInstance>() ??
                k.Adapt<SpecialistInstance>() ??
                k.Adapt<WearableInstance>() ??
                k.Adapt<UsableInstance>() ??
                (IItemInstance) k.Adapt<ItemInstance>();

            item.Item = _items.Find(s => s.VNum == k.ItemVNum).Adapt<Item.Item>();
            LoadHandlers(item);
            return item;
        }

        public IItemInstance Create(short itemToCreateVNum, long characterId) =>
            Create(itemToCreateVNum, characterId, 1);

        public IItemInstance Create(short itemToCreateVNum, long characterId, short amount) =>
            Create(itemToCreateVNum, characterId, amount, 0);

        public IItemInstance Create(short itemToCreateVNum, long characterId, short amount, sbyte rare) =>
            Create(itemToCreateVNum, characterId, amount, rare, 0);

        public IItemInstance Create(short itemToCreateVNum, long characterId, short amount, sbyte rare,
            byte upgrade) => Create(itemToCreateVNum, characterId, amount, rare, upgrade, 0);

        public IItemInstance Create(short itemToCreateVNum, long characterId, short amount, sbyte rare,
            byte upgrade, byte design)
        {
            var item = Generate(itemToCreateVNum, characterId, amount, rare, upgrade, design);
            LoadHandlers(item);
            return item;
        }

        private void LoadHandlers(IItemInstance itemInstance)
        {
            var handlersRequest = new Subject<RequestData<Tuple<IItemInstance, UseItemPacket>>>();
            _handlers.ForEach(handler =>
            {
                if (handler.Condition(itemInstance.Item))
                {
                    handlersRequest.Subscribe(handler.Execute);
                }
            });
            itemInstance.Requests = handlersRequest;
        }

        public IItemInstance Generate(short itemToCreateVNum, long characterId, short amount, sbyte rare,
            byte upgrade, byte design)
        {
            Item.Item itemToCreate = _items.Find(s => s.VNum == itemToCreateVNum).Adapt<Item.Item>();
            switch (itemToCreate.Type)
            {
                case PocketType.Miniland:
                    return new ItemInstance(itemToCreate)
                    {
                        CharacterId = characterId,
                        Amount = amount,
                        DurabilityPoint = itemToCreate.MinilandObjectPoint / 2
                    };

                case PocketType.Equipment:
                    switch (itemToCreate.ItemType)
                    {
                        case ItemType.Specialist:
                            return new SpecialistInstance(itemToCreate)
                            {
                                SpLevel = 1,
                                Amount = amount,
                                CharacterId = characterId,
                                Design = design,
                                Upgrade = upgrade
                            };
                        case ItemType.Box:
                            return new BoxInstance(itemToCreate)
                            {
                                Amount = amount,
                                CharacterId = characterId,
                                Upgrade = upgrade,
                                Design = design
                            };
                        default:
                            var wear = new WearableInstance(itemToCreate)
                            {
                                Amount = amount,
                                Rare = rare,
                                CharacterId = characterId,
                                Upgrade = upgrade,
                                Design = design
                            };
                            if (wear.Rare > 0)
                            {
                                wear.SetRarityPoint();
                            }

                            return wear;
                    }

                default:
                    return new ItemInstance(itemToCreate)
                    {
                        Type = itemToCreate.Type,
                        Amount = amount,
                        CharacterId = characterId
                    };
            }
        }
    }
}