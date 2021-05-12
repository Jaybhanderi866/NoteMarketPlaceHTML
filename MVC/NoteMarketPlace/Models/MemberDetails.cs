using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NoteMarketPlace.Models;
using PagedList;

namespace NoteMarketPlace.Models
{
    public class MemberDetails
    {
        public UserProfileDetail UserProfile { get; set; }
        public List<BuyerRequest> noteRequest { get; set; }
        public IPagedList<NoteDetail> pagedList { get; set; }
    }
}
