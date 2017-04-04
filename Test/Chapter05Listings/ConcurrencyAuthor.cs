﻿// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace test.Chapter05Listings
{
    public class ConcurrencyAuthor
    {
        public int ConcurrencyAuthorId { get; set; }

        public string Name { get; set; }

        [Timestamp] //#A
        public byte[] RowVersion { get; set; }
    }
    /***********************************************
    #A This marks the property RowVersion as as a timestamp. This will cause the database server to mark it as a ROWVERSION and EF Core will check this when updating to see if this has changed
     * *********************************************/
}