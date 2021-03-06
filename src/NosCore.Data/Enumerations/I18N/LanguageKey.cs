//  __  _  __    __   ___ __  ___ ___  
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

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace NosCore.Data.Enumerations.I18N
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LogLanguageKey
    {
        UNKNOWN,
        DONE,
        PARSE_ALL,
        PARSE_MAPS,
        PARSE_MAPTYPES,
        PARSE_ACCOUNTS,
        PARSE_PORTALS,
        PARSE_TIMESPACES,
        PARSE_ITEMS,
        PARSE_NPCMONSTERS,
        PARSE_NPCMONSTERDATA,
        PARSE_CARDS,
        PARSE_SKILLS,
        PARSE_MAPNPCS,
        PARSE_MONSTERS,
        PARSE_SHOPS,
        PARSE_TELEPORTERS,
        PARSE_SHOPITEMS,
        PARSE_SHOPSKILLS,
        PARSE_RECIPES,
        PARSE_QUESTS,
        I18N_ACTDESC_PARSED,
        I18N_CARD_PARSED,
        I18N_BCARD_PARSED,
        I18N_ITEM_PARSED,
        I18N_MAPIDDATA_PARSED,
        I18N_MAPPOINTDATA_PARSED,
        I18N_MPCMONSTER_PARSED,
        I18N_NPCMONSTERTALK_PARSED,
        I18N_QUEST_PARSED,
        I18N_SKILL_PARSED,
        AUTH_ERROR,
        MASTER_SERVER_PING,
        MASTER_SERVER_PING_FAILED,
        REGISTRED_ON_MASTER,
        AUTHENTICATED_SUCCESS,
        AUTHENTICATED_ERROR,
        DATABASE_INITIALIZED,
        DATABASE_NOT_UPTODATE,
        CLIENT_DISCONNECTED,
        CHARACTER_NOT_INIT,
        ERROR_CHANGE_MAP,
        AUTH_INCORRECT,
        FORCED_DISCONNECTION,
        CLIENT_CONNECTED,
        CLIENT_ARRIVED,
        CORRUPTED_KEEPALIVE,
        INVALID_PASSWORD,
        INVALID_ACCOUNT,
        ACCOUNT_ARRIVED,
        SUCCESSFULLY_LOADED,
        MASTER_SERVER_RETRY,
        LISTENING_PORT,
        ENTER_PATH,
        AT_LEAST_ONE_FILE_MISSING,
        CARDS_PARSED,
        ITEMS_PARSED,
        MAPS_PARSED,
        PORTALS_PARSED,
        MAPS_LOADED,
        NO_MAP,
        MAPMONSTERS_LOADED,
        CORRUPT_PACKET,
        HANDLER_ERROR,
        HANDLER_NOT_FOUND,
        SELECT_MAPID,
        WRONG_SELECTED_MAPID,
        PARSE_I18N,
        NPCMONSTERS_PARSED,
        PARSE_DROPS,
        MAPTYPES_PARSED,
        RESPAWNTYPE_PARSED,
        SKILLS_PARSED,
        NPCS_PARSED,
        MONSTERS_PARSED,
        MAPNPCS_LOADED,
        SAVING_ALL,
        ITEMS_LOADED,
        UNKNOWN_PICKERTYPE,
        POCKETTYPE_UNKNOWN,
        NPCMONSTERS_LOADED,
        ENCODE_ERROR,
        VISUALTYPE_UNKNOWN,
        UNKNOWN_EQUIPMENTTYPE,
        ERROR_DECODING,
        LANGUAGE_MISSING,
        INVITETYPE_UNKNOWN,
        GROUPREQUESTTYPE_UNKNOWN,
        ITEMTYPE_UNKNOWN,
        UNKWNOWN_RECEIVERTYPE,
        NO_SPECIAL_PROPERTIES_WEARABLE,
        CANNOT_TRADE_NOT_TRADABLE_ITEM,
        CANT_USE_ITEM_IN_SHOP,
        TRY_REMOVE_FAILED,
        PACKET_WRONG_FORMAT,
        PACKET_HEADER_CANNOT_EMPTY,
        NOT_CONVERT_VALUE,
        USE_SP_WITHOUT_SP_ERROR,
        NOT_ENOUGH_GOLD,
        INVALID_EXCHANGE,
        PLAYER_IN_SHOP,
        USER_NOT_IN_EXCHANGE,
        INVALID_EXCHANGE_LIST,
        AMOUNT_SPLITTED_SUBPACKET_INCORRECT,
        SHOPS_PARSED,
        SHOPITEMS_PARSED,
        SHOPITEMS_LOADED,
        SHOPS_LOADED,
        CONNECTION_LOST,
        CHANNEL_WILL_EXIT,
        CANT_MOVE_ITEM_IN_SHOP,
        EXCEPTION,
        VISUALENTITY_DOES_NOT_EXIST,
        ALREADY_EXCHANGE,
        CANT_FIND_CHARACTER,
        LOADING_MAPINSTANCES
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LanguageKey
    {
        ALREADY_TAKEN,
        INVALID_CHARNAME,
        BAD_PASSWORD,
        SUPPORT,
        [UsedImplicitly] ADVENTURER, //keep LanguageKey.ADVENTURER
        [UsedImplicitly] SWORDMAN, //keep LanguageKey.SWORDMAN
        [UsedImplicitly] ARCHER, //keep LanguageKey.ARCHER
        [UsedImplicitly] MAGICIAN, //keep LanguageKey.MAGICIAN
        [UsedImplicitly] MARTIALARTIST, //keep LanguageKey.MARTIALARTIST
        CHANNEL,
        ADMINISTRATOR,
        CHARACTER_OFFLINE,
        SEND_MESSAGE_TO_CHARACTER,
        NO_ITEM,
        NOT_ENOUGH_PLACE,
        ITEM_ACQUIRED,
        ASK_TO_DELETE,
        SURE_TO_DELETE,
        ITEM_NOT_DROPPABLE_HERE,
        DROP_MAP_FULL,
        BAD_DROP_AMOUNT,
        ITEM_NOT_DROPPABLE,
        CANT_FIND_CHARACTER,
        FRIENDLIST_FULL,
        ALREADY_FRIEND,
        FRIEND_REQUEST_BLOCKED,
        MAX_GOLD,
        ITEM_ACQUIRED_LOD,
        SP_POINTSADDED,
        FRIEND_ADD,
        FRIEND_ADDED,
        FRIEND_REJECTED,
        CANT_BLOCK_FRIEND,
        NOT_IN_BLACKLIST,
        BLACKLIST_ADDED,
        BLACKLIST_BLOCKED,
        FRIEND_DELETED,
        FRIEND_OFFLINE,
        GROUP_FULL,
        ALREADY_IN_GROUP,
        GROUP_BLOCKED,
        INVITED_YOU_GROUP,
        INVITED_GROUP_SHARE,
        GROUP_SHARE_INFO,
        JOINED_GROUP,
        GROUP_ADMIN,
        GROUP_REFUSED,
        SHARED_REFUSED,
        ACCEPTED_SHARE,
        NEW_LEADER,
        LEAVE_GROUP,
        GROUP_LEFT,
        GROUP_CLOSED,
        GROUP_INVITE,
        UNABLE_TO_REQUEST_GROUP,
        MAP_DONT_EXIST,
        USER_NOT_CONNECTED,
        FRIEND_REQUEST_SENT,
        ALREADY_EXCHANGE,
        ALREADY_BLACKLISTED,
        USER_IS_NOT_A_FRIEND,
        NOT_YOUR_ITEM,
        EXCHANGE_BLOCKED,
        HAS_SHOP_OPENED,
        YOU_ASK_FOR_EXCHANGE,
        INCOMING_EXCHANGE,
        EXCHANGE_REFUSED,
        LEVEL_CHANGED,
        JOB_LEVEL_CHANGED,
        HERO_LEVEL_CHANGED,
        ITEM_NOT_TRADABLE,
        NOT_ADVENTURER,
        TOO_LOW_LEVEL,
        EQ_NOT_EMPTY,
        CANT_CHANGE_SAME_CLASS,
        BAD_EQUIPMENT,
        SP_BLOCKED,
        LOW_JOB_LVL,
        CANT_EQUIP_DESTROYED_SP,
        BAD_FAIRY,
        CLASS_CHANGED,
        INVENTORY_FULL,
        BANK_FULL,
        IN_WAITING_FOR,
        SP_INLOADING,
        ASK_BIND,
        NO_SP,
        REMOVE_VEHICLE,
        SP_NOPOINTS,
        LOW_REP,
        TRANSFORM_DISAPPEAR,
        STAY_TIME,
        REPUTATION_CHANGED,
        REPUT_DECREASED,
        BUY_ITEM_VALID,
        NOT_ENOUGH_REPUT,
        NOT_ENOUGH_MONEY,
        ITEM_NOT_SOLDABLE,
        SELL_ITEM_VALIDE,
        SHOP_NOT_ALLOWED,
        SHOP_NOT_ALLOWED_IN_RAID,
        SHOP_NEAR_PORTAL,
        SHOP_ONLY_TRADABLE_ITEMS,
        SHOP_EMPTY,
        SHOP_PRIVATE_SHOP,
        SHOP_OPEN,
        BUY_ITEM_FROM,
        TOO_RICH_SELLER,
        UPDATE_GOLD,
        SP_ADDPOINTS_FULL,
    }
}