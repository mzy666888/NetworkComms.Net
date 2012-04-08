﻿//  Copyright 2011 Marc Fletcher, Matthew Dean
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace DistributedFileSystem
{
    public enum DFSLinkMode
    {
        /// <summary>
        /// Only items existing at both ends are linked
        /// </summary>
        LinkOnly,

        /// <summary>
        /// All items existing on the target peer are retrived and held locally. Any items already on local will be linked.
        /// </summary>
        LinkAndRepeat,
    }

    [ProtoContract]
    public class DFSLinkRequest
    {
        /// <summary>
        /// If this linkRequest object has been sent in reply to a linkRequest this boolean is true
        /// </summary>
        [ProtoMember(1)]
        public bool LinkRequestReply { get; private set; }

        /// <summary>
        /// The DFS items which can possibly be linked
        /// </summary>
        [ProtoMember(2)]
        public long[] AvailableItemCheckSums { get; private set; }

        private DFSLinkRequest() 
        {
            if (AvailableItemCheckSums == null) AvailableItemCheckSums = new long[0];
        }

        /// <summary>
        /// Create an item link request
        /// </summary>
        /// <param name="availableItemCheckSums"></param>
        /// <param name="linkRequestReply"></param>
        public DFSLinkRequest(long[] availableItemCheckSums, bool linkRequestReply = false)
        {
            this.LinkRequestReply = linkRequestReply;
            this.AvailableItemCheckSums = availableItemCheckSums;
        }
    }
}