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
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using NosCore.Core.Extensions;
using NosCore.Core.Networking;
using NosCore.Data.Enumerations;

namespace NosCore.Core.Encryption
{
    public class WorldDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        private RegionType _region;
        private int _sessionId;

        private string DecryptPrivate(string str)
        {
            var receiveData = new List<byte>();
            char[] table = {' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n'};
            int count;
            for (count = 0; count < str.Length; count++)
            {
                if (str[count] <= 0x7A)
                {
                    int len = str[count];

                    for (var i = 0; i < len; i++)
                    {
                        count++;

                        try
                        {
                            receiveData.Add(unchecked((byte) (str[count] ^ 0xFF)));
                        }
                        catch
                        {
                            receiveData.Add(255);
                        }
                    }
                }
                else
                {
                    int len = str[count];
                    len &= 0x7F;

                    for (var i = 0; i < len; i++)
                    {
                        count++;
                        int highbyte;
                        try
                        {
                            highbyte = str[count];
                        }
                        catch
                        {
                            highbyte = 0;
                        }

                        highbyte &= 0xF0;
                        highbyte >>= 0x4;

                        int lowbyte;
                        try
                        {
                            lowbyte = str[count];
                        }
                        catch
                        {
                            lowbyte = 0;
                        }

                        lowbyte &= 0x0F;

                        if (highbyte != 0x0 && highbyte != 0xF)
                        {
                            receiveData.Add(unchecked((byte) table[highbyte - 1]));
                            i++;
                        }

                        if (lowbyte != 0x0 && lowbyte != 0xF)
                        {
                            receiveData.Add(unchecked((byte) table[lowbyte - 1]));
                        }
                    }
                }
            }

            return _region.GetEncoding().GetString(receiveData.ToArray());
        }

        public string DecryptCustomParameter(byte[] str)
        {
            try
            {
                var encryptedStringBuilder = new StringBuilder();
                for (var i = 1; i < str.Length; i++)
                {
                    if (Convert.ToChar(str[i]) == 0xE)
                    {
                        return encryptedStringBuilder.ToString();
                    }

                    var firstbyte = Convert.ToInt32(str[i] - 0xF);
                    var secondbyte = firstbyte;
                    secondbyte &= 0xF0;
                    firstbyte = Convert.ToInt32(firstbyte - secondbyte);
                    secondbyte >>= 0x4;

                    switch (secondbyte)
                    {
                        case 0:
                        case 1:
                            encryptedStringBuilder.Append(' ');
                            break;

                        case 2:
                            encryptedStringBuilder.Append('-');
                            break;

                        case 3:
                            encryptedStringBuilder.Append('.');
                            break;

                        default:
                            secondbyte += 0x2C;
                            encryptedStringBuilder.Append(Convert.ToChar(secondbyte));
                            break;
                    }

                    switch (firstbyte)
                    {
                        case 0:

                        case 1:
                            encryptedStringBuilder.Append(' ');
                            break;

                        case 2:
                            encryptedStringBuilder.Append('-');
                            break;

                        case 3:
                            encryptedStringBuilder.Append('.');
                            break;

                        default:
                            firstbyte += 0x2C;
                            encryptedStringBuilder.Append(Convert.ToChar(firstbyte));
                            break;
                    }
                }

                return encryptedStringBuilder.ToString();
            }
            catch (OverflowException)
            {
                return string.Empty;
            }
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            var encryptedString = "";
            var mapper = SessionFactory.Instance.Sessions[context.Channel.Id.AsLongText()];
            _region = mapper.RegionType;
            _sessionId = mapper.SessionId;
            var str = ((Span<byte>) message.Array).Slice(message.ArrayOffset, message.ReadableBytes).ToArray();
            if (_sessionId == 0)
            {
                output.Add(DecryptCustomParameter(str));
                return;
            }

            var sessionKey = _sessionId & 0xFF;
            var sessionNumber = unchecked((byte) (_sessionId >> 6));
            sessionNumber &= 0xFF;
            sessionNumber &= unchecked((byte) 0x80000003);

            switch (sessionNumber)
            {
                case 0:
                    encryptedString =
                        (from character in str let firstbyte = unchecked((byte) (sessionKey + 0x40))
                            select unchecked((byte) (character - firstbyte))).Aggregate(encryptedString,
                            (current, highbyte) => current + (char) highbyte);
                    break;

                case 1:
                    encryptedString =
                        (from character in str let firstbyte = unchecked((byte) (sessionKey + 0x40))
                            select unchecked((byte) (character + firstbyte))).Aggregate(encryptedString,
                            (current, highbyte) => current + (char) highbyte);
                    break;

                case 2:
                    encryptedString =
                        (from character in str let firstbyte = unchecked((byte) (sessionKey + 0x40))
                            select unchecked((byte) ((character - firstbyte) ^ 0xC3))).Aggregate(encryptedString,
                            (current, highbyte) => current + (char) highbyte);
                    break;

                case 3:
                    encryptedString =
                        (from character in str let firstbyte = unchecked((byte) (sessionKey + 0x40))
                            select unchecked((byte) ((character + firstbyte) ^ 0xC3))).Aggregate(encryptedString,
                            (current, highbyte) => current + (char) highbyte);
                    break;

                default:
                    encryptedString += (char) 0xF;
                    break;
            }

            var temp = encryptedString.Split((char) 0xFF);
            var save = new StringBuilder();

            for (var i = 0; i < temp.Length; i++)
            {
                save.Append(DecryptPrivate(temp[i]));
                if (i < temp.Length - 2)
                {
                    save.Append((char) 0xFF);
                }
            }

            output.Add(save.ToString());
        }
    }
}