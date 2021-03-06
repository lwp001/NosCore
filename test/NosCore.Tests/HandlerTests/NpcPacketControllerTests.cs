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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Controllers;
using NosCore.Core;
using NosCore.Core.Encryption;
using NosCore.Core.Networking;
using NosCore.Core.Serializing;
using NosCore.Data;
using NosCore.Data.AliveEntities;
using NosCore.Data.StaticEntities;
using NosCore.Data.WebApi;
using NosCore.Database;
using NosCore.GameObject;
using NosCore.GameObject.Map;
using NosCore.GameObject.Networking;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.Packets.ClientPackets;
using Character = NosCore.GameObject.Character;
using NosCore.Configuration;
using NosCore.Core.I18N;
using NosCore.Data.Enumerations;
using NosCore.Data.Enumerations.Character;
using NosCore.Data.Enumerations.Group;
using NosCore.Data.Enumerations.I18N;
using NosCore.Data.Enumerations.Items;
using NosCore.Data.Enumerations.Map;
using NosCore.Database.DAL;
using NosCore.GameObject.ComponentEntities.Interfaces;
using NosCore.GameObject.Providers.InventoryService;
using NosCore.GameObject.Providers.ItemProvider;
using NosCore.GameObject.Providers.ItemProvider.Item;
using NosCore.GameObject.Providers.MapInstanceProvider;
using NosCore.GameObject.Providers.MapItemProvider;
using NosCore.GameObject.Providers.NRunProvider;
using NosCore.Packets.ServerPackets;
using Serilog;

namespace NosCore.Tests.HandlerTests
{
    [TestClass]
    public class NpcPacketControllerTests
    {
        private static readonly ILogger _logger = Logger.GetLoggerConfiguration().CreateLogger();
        private readonly IGenericDao<MapDto> _mapDao = new GenericDao<Database.Entities.Map, MapDto>(_logger);
        private readonly IGenericDao<AccountDto> _accountDao = new GenericDao<Database.Entities.Account, AccountDto>(_logger);
        private readonly IGenericDao<CharacterDto> _characterDao = new GenericDao<Database.Entities.Character, CharacterDto>(_logger);
        private readonly IGenericDao<CharacterRelationDto> _characterRelationDao = new GenericDao<Database.Entities.CharacterRelation, CharacterRelationDto>(_logger);
        private readonly IGenericDao<PortalDto> _portalDao = new GenericDao<Database.Entities.Portal, PortalDto>(_logger);
        private readonly IGenericDao<MapMonsterDto> _mapMonsterDao = new GenericDao<Database.Entities.MapMonster, MapMonsterDto>(_logger);
        private readonly IGenericDao<MapNpcDto> _mapNpcDao = new GenericDao<Database.Entities.MapNpc, MapNpcDto>(_logger);
        private readonly IGenericDao<IItemInstanceDto> _itemInstanceDao = new ItemInstanceDao(_logger);
        private readonly Map _map = new Map
        {
            MapId = 0,
            Name = "testMap",
            Data = new byte[]
            {
                8, 0, 8, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 1, 1, 1, 0, 0, 0, 0,
                0, 1, 1, 1, 0, 0, 0, 0,
                0, 1, 1, 1, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        private readonly Map _mapShop = new Map
        {
            MapId = 1,
            Name = "shopMap",
            ShopAllowed = true,
            Data = new byte[]
            {
                8, 0, 8, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 1, 1, 1, 0, 0, 0, 0,
                0, 1, 1, 1, 0, 0, 0, 0,
                0, 1, 1, 1, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        private readonly MShopPacket shopPacket = new MShopPacket
        {
            Type = CreateShopPacketType.Open,
            ItemList = new List<MShopItemSubPacket>
            {
                new MShopItemSubPacket {Type = PocketType.Etc, Slot = 0, Amount = 1, Price = 10000},
                new MShopItemSubPacket {Type = PocketType.Etc, Slot = 1, Amount = 2, Price = 20000},
                new MShopItemSubPacket {Type = PocketType.Etc, Slot = 2, Amount = 3, Price = 30000},
            },
            Name = "TEST SHOP"
        };

        private NpcPacketController _handler;

        MapInstanceProvider _instanceProvider;
        private ClientSession _session;

        [TestInitialize]
        public void Setup()
        {
            TypeAdapterConfig.GlobalSettings.ForDestinationType<IInitializable>().AfterMapping(dest => Task.Run(() => dest.Initialize()));
            PacketFactory.Initialize<NoS0575Packet>();
            Broadcaster.Reset();
            var contextBuilder =
                new DbContextOptionsBuilder<NosCoreContext>().UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString());
            DataAccessHelper.Instance.InitializeForTest(contextBuilder.Options);
            var map = new MapDto { MapId = 1 };
            _mapDao.InsertOrUpdate(ref map);
            var account = new AccountDto { Name = "AccountTest", Password = "test".ToSha512() };
            _accountDao.InsertOrUpdate(ref account);
            WebApiAccess.RegisterBaseAdress();
            WebApiAccess.Instance.MockValues =
                new Dictionary<WebApiRoute, object>
                {
                    {WebApiRoute.Channel, new List<ChannelInfo> {new ChannelInfo()}},
                    {WebApiRoute.ConnectedAccount, new List<ConnectedAccount>()}
                };

            var conf = new WorldConfiguration { BackpackSize = 3, MaxItemAmount = 999, MaxGoldAmount = 999_999_999 };
            var _chara = new Character(new InventoryService(new List<ItemDto>(), conf, _logger), null, null, _characterRelationDao, _characterDao, _itemInstanceDao, _accountDao, _logger)
            {
                CharacterId = 1,
                Name = "TestExistingCharacter",
                Slot = 1,
                AccountId = account.AccountId,
                MapId = 1,
                State = CharacterState.Active
            };

            ItemProvider itemProvider = new ItemProvider(new List<ItemDto>(),
                new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());
            _instanceProvider = new MapInstanceProvider(new List<MapDto>{ _map, _mapShop },
                new MapItemProvider(new List<IHandler<MapItem, Tuple<MapItem, GetPacket>>>()),
                _mapNpcDao,
                _mapMonsterDao, _portalDao, new Adapter(), _logger);
            _instanceProvider.Initialize();
            var channelMock = new Mock<IChannel>();
            _session = new ClientSession(null,
                new List<PacketController> { new DefaultPacketController(null, _instanceProvider, null, _logger) },
                _instanceProvider, null, _logger);
            _session.RegisterChannel(channelMock.Object);
            _session.InitializeAccount(account);
            _session.SessionId = 1;
            _handler = new NpcPacketController(conf,
                new NrunProvider(
                    new List<IHandler<Tuple<IAliveEntity, NrunPacket>, Tuple<IAliveEntity, NrunPacket>>>()), _logger);
            _handler.RegisterSession(_session);
            _session.SetCharacter(_chara);
            var mapinstance = _instanceProvider.GetBaseMapById(0);
            _session.Character.Account = account;
            _session.Character.MapInstance = mapinstance;
            _session.Character.MapInstance.Portals = new List<Portal>
            {
                new Portal
                {
                    DestinationMapId = _map.MapId,
                    Type = PortalType.Open,
                    SourceMapInstanceId = mapinstance.MapInstanceId,
                    DestinationMapInstanceId = mapinstance.MapInstanceId,
                    DestinationX = 5,
                    DestinationY = 5,
                    PortalId = 1,
                    SourceMapId = _map.MapId,
                    SourceX = 0,
                    SourceY = 0,
                }
            };

            Broadcaster.Instance.RegisterSession(_session);
        }

        [TestMethod]
        public void UserCanNotCreateShopCloseToPortal()
        {
            _handler.CreateShop(shopPacket);
            var packet = (MsgPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NEAR_PORTAL, _session.Account.Language));
            Assert.IsNull(_session.Character.Shop);
        }

        [TestMethod]
        public void UserCanNotCreateShopInTeam()
        {
            _session.Character.PositionX = 7;
            _session.Character.PositionY = 7;
            _session.Character.Group = new Group(GroupType.Team);
            _handler.CreateShop(shopPacket);
            var packet = (MsgPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NOT_ALLOWED_IN_RAID, _session.Account.Language));
            Assert.IsNull(_session.Character.Shop);
        }

        [TestMethod]
        public void UserCanCreateShopInGroup()
        {
            _session.Character.PositionX = 7;
            _session.Character.PositionY = 7;
            _session.Character.Group = new Group(GroupType.Group);
            _handler.CreateShop(shopPacket);
            var packet = (MsgPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message !=
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NOT_ALLOWED_IN_RAID, _session.Account.Language));
        }

        [TestMethod]
        public void UserCanNotCreateShopInNotShopAllowedMaps()
        {
            _session.Character.PositionX = 7;
            _session.Character.PositionY = 7;
            _handler.CreateShop(shopPacket);
            var packet = (MsgPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NOT_ALLOWED, _session.Account.Language));
            Assert.IsNull(_session.Character.Shop);
        }


        [TestMethod]
        public void UserCanNotCreateShopWithMissingItem()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1));
            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.CreateShop(shopPacket);
            Assert.IsNull(_session.Character.Shop);
        }


        [TestMethod]
        public void UserCanNotCreateShopWithMissingAmountItem()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 2);

            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.CreateShop(shopPacket);
            Assert.IsNull(_session.Character.Shop);
            var packet = (SayPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_ONLY_TRADABLE_ITEMS, _session.Account.Language));
        }

        [TestMethod]
        public void UserCanCreateShop()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsTradable = true},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3), PocketType.Etc, 2);

            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.CreateShop(shopPacket);
            Assert.IsNotNull(_session.Character.Shop);
        }

        public void UserCanNotCreateShopInExchange()
        {
            _session.Character.InExchangeOrTrade = true;
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsTradable = true},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3), PocketType.Etc, 2);

            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.CreateShop(shopPacket);
            Assert.IsNull(_session.Character.Shop);
        }

        [TestMethod]
        public void UserCanNotCreateEmptyShop()
        {
            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.CreateShop(new MShopPacket
            {
                Type = CreateShopPacketType.Open,
                ItemList = new List<MShopItemSubPacket>(),
                Name = "TEST SHOP"
            });
            Assert.IsNull(_session.Character.Shop);
            var packet = (SayPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_EMPTY, _session.Account.Language));
        }

        [TestMethod]
        public void UserCanNotSellInExchange()
        {
            _session.Character.InExchangeOrTrade = true;
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsTradable = true},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3), PocketType.Etc, 2);

            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.SellShop(new SellPacket { Slot = 0, Amount = 1, Data = (short)PocketType.Etc });
            Assert.IsTrue(_session.Character.Gold == 0);
            Assert.IsNotNull(_session.Character.Inventory.LoadBySlotAndType<IItemInstance>(0, PocketType.Etc));
        }

        [TestMethod]
        public void UserCanNotSellNotSoldable()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = false},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3), PocketType.Etc, 2);

            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.SellShop(new SellPacket { Slot = 0, Amount = 1, Data = (short)PocketType.Etc });
            var packet = (SMemoPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.ITEM_NOT_SOLDABLE, _session.Account.Language));
            Assert.IsTrue(_session.Character.Gold == 0);
            Assert.IsNotNull(_session.Character.Inventory.LoadBySlotAndType<IItemInstance>(0, PocketType.Etc));
        }

        [TestMethod]
        public void UserCanSell()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, Price = 500000},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3), PocketType.Etc, 2);

            _session.Character.MapInstance = _instanceProvider.GetBaseMapById(1);
            _handler.SellShop(new SellPacket { Slot = 0, Amount = 1, Data = (short)PocketType.Etc });
            Assert.IsTrue(_session.Character.Gold > 0);
            Assert.IsNull(_session.Character.Inventory.LoadBySlotAndType<IItemInstance>(0, PocketType.Etc));
        }

        [TestMethod]
        public void UserCanNotShopNonExistingSlot()
        {
            _session.Character.Gold = 9999999999;
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, Price = 500000},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            var list = new ConcurrentDictionary<int, ShopItem>();
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = itemBuilder.Create(1, -1), Type = 0 });
            var shop = new Shop
            {
                ShopItems = list
            };
            _session.Character.Buy(shop, 1, 99);
            Assert.IsNull(_session.LastPacket);
        }

        [TestMethod]
        public void UserCantShopMoreThanQuantityNonExistingSlot()
        {
            _session.Character.Gold = 9999999999;
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, Price = 500000},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            var list = new ConcurrentDictionary<int, ShopItem>();
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = itemBuilder.Create(1, -1), Type = 0, Amount = 98 });
            var shop = new Shop
            {
                ShopItems = list
            };
            _session.Character.Buy(shop, 0, 99);
            Assert.IsNull(_session.LastPacket);
        }

        [TestMethod]
        public void UserCantShopWithoutMoney()
        {
            _session.Character.Gold = 500000;
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, Price = 500000},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            var list = new ConcurrentDictionary<int, ShopItem>();
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = itemBuilder.Create(1, -1), Type = 0 });
            var shop = new Shop
            {
                ShopItems = list
            };
            _session.Character.Buy(shop, 0, 99);

            var packet = (SMemoPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.NOT_ENOUGH_MONEY, _session.Account.Language));
        }

        [TestMethod]
        public void UserCantShopWithoutReput()
        {
            _session.Character.Reput = 500000;
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, ReputPrice = 500000},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());

            var list = new ConcurrentDictionary<int, ShopItem>();
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = itemBuilder.Create(1, -1), Type = 0 });
            var shop = new Shop
            {
                ShopItems = list
            };
            _session.Character.Buy(shop, 0, 99);

            var packet = (SMemoPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.NOT_ENOUGH_REPUT, _session.Account.Language));
        }

        [TestMethod]
        public void UserCantShopWithoutPlace()
        {
            _session.Character.Gold = 500000;

            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, Price = 1},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());
            _session.Character.ItemProvider = itemBuilder;
            var list = new ConcurrentDictionary<int, ShopItem>();
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = itemBuilder.Create(1, -1), Type = 0 });
            var shop = new Shop
            {
                ShopItems = list
            };
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1, 999), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2, 999), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3, 999), PocketType.Etc, 2);

            _session.Character.Buy(shop, 0, 999);
            var packet = (MsgPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.NOT_ENOUGH_PLACE, _session.Account.Language));
        }

        [TestMethod]
        public void UserCanShop()
        {
            _session.Character.Gold = 500000;

            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, Price = 1},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());
            _session.Character.ItemProvider = itemBuilder;
            var list = new ConcurrentDictionary<int, ShopItem>();
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = itemBuilder.Create(1, -1), Type = 0 });
            var shop = new Shop
            {
                ShopItems = list
            };
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1, 999), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2, 999), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3, 1), PocketType.Etc, 2);

            _session.Character.Buy(shop, 0, 998);
            Assert.IsTrue(_session.Character.Inventory.All(s => s.Value.Amount == 999));
            Assert.IsTrue(_session.Character.Gold == 499002);
        }

        [TestMethod]
        public void UserCanShopReput()
        {
            _session.Character.Reput = 500000;

            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, ReputPrice = 1},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());
            _session.Character.ItemProvider = itemBuilder;
            var list = new ConcurrentDictionary<int, ShopItem>();
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = itemBuilder.Create(1, -1), Type = 0 });
            var shop = new Shop
            {
                ShopItems = list
            };
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1, 999), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2, 999), PocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 3, 1), PocketType.Etc, 2);

            _session.Character.Buy(shop, 0, 998);
            Assert.IsTrue(_session.Character.Inventory.All(s => s.Value.Amount == 999));
            Assert.IsTrue(_session.Character.Reput == 499002);
        }

        private ClientSession PrepareSessionShop()
        {
            var conf = new WorldConfiguration { BackpackSize = 3, MaxItemAmount = 999, MaxGoldAmount = 999_999_999 };
            var session2 = new ClientSession(conf,
                new List<PacketController> { new DefaultPacketController(null, _instanceProvider, null, _logger) },
                _instanceProvider, null, _logger);
            var channelMock = new Mock<IChannel>();
            session2.RegisterChannel(channelMock.Object);
            var account = new AccountDto { Name = "AccountTest", Password = "test".ToSha512() };
            session2.InitializeAccount(account);
            session2.SessionId = 1;

            _handler = new NpcPacketController(conf,
                new NrunProvider(
                    new List<IHandler<Tuple<IAliveEntity, NrunPacket>, Tuple<IAliveEntity, NrunPacket>>>()), _logger);
            _handler.RegisterSession(session2);
            session2.SetCharacter(new Character(new InventoryService(new List<ItemDto>(), conf, _logger), null, null, null, null, null, null, _logger)
            {
                CharacterId = 1,
                Name = "chara2",
                Slot = 1,
                AccountId = 1,
                MapId = 1,
                State = CharacterState.Active
            });
            var mapinstance = _instanceProvider.GetBaseMapById(0);
            session2.Character.Account = account;
            session2.Character.MapInstance = _instanceProvider.GetBaseMapById(0);
            session2.Character.MapInstance = mapinstance;

            _session.Character.Gold = 500000;
            var items = new List<ItemDto>
            {
                new Item {Type = PocketType.Etc, VNum = 1, IsSoldable = true, Price = 1},
            };
            var itemBuilder = new ItemProvider(items, new List<IHandler<Item, Tuple<IItemInstance, UseItemPacket>>>());
            _session.Character.ItemProvider = itemBuilder;
            var list = new ConcurrentDictionary<int, ShopItem>();
            var it = itemBuilder.Create(1, 1, 999);
            session2.Character.Inventory.AddItemToPocket(it, PocketType.Etc, 0);
            list.TryAdd(0, new ShopItem { Slot = 0, ItemInstance = it, Type = 0, Price = 1, Amount = 999 });
            list.TryAdd(1, new ShopItem { Slot = 1, ItemInstance = it, Type = 0, Price = 1, Amount = 500 });
            session2.Character.Shop = new Shop
            {
                Session = session2,
                ShopItems = list
            };
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 1, 999), PocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(itemBuilder.Create(1, 2, 999), PocketType.Etc, 1);
            return session2;
        }

        [TestMethod]
        public void UserCanShopFromSession()
        {
            var session2 = PrepareSessionShop();
            _session.Character.Buy(session2.Character.Shop, 0, 999);
            Assert.IsTrue(session2.Character.Gold == 999);
            Assert.IsTrue(session2.Character.Inventory.CountItem(1) == 0);
        }

        [TestMethod]
        public void UserCanShopFromSessionPartial()
        {
            var session2 = PrepareSessionShop();
            _session.Character.Buy(session2.Character.Shop, 0, 998);
            Assert.IsTrue(session2.Character.Gold == 998);
            Assert.IsTrue(session2.Character.Inventory.CountItem(1) == 1);
        }

        [TestMethod]
        public void UserCanNotShopMoreThanShop()
        {
            var session2 = PrepareSessionShop();
            _session.Character.Buy(session2.Character.Shop, 1, 501);
            Assert.IsTrue(session2.Character.Gold == 0);
            Assert.IsTrue(session2.Character.Inventory.CountItem(1) == 999);
        }

        [TestMethod]
        public void UserCanShopFull()
        {
            var session2 = PrepareSessionShop();
            _session.Character.Buy(session2.Character.Shop, 1, 500);
            Assert.IsTrue(session2.Character.Gold == 500);
            Assert.IsTrue(session2.Character.Inventory.CountItem(1) == 499);
        }

        [TestMethod]
        public void UserCanNotShopTooRich()
        {
            var session2 = PrepareSessionShop();
            session2.Character.Gold = 999_999_999;
            _session.Character.Buy(session2.Character.Shop, 0, 999);
            Assert.IsTrue(session2.Character.Gold == 999_999_999);
            Assert.IsTrue(session2.Character.Inventory.CountItem(1) == 999);

            var packet = (SMemoPacket)_session.LastPacket;
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.TOO_RICH_SELLER, _session.Account.Language));
        }
    }
}