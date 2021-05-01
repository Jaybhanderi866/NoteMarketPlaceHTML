using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NoteMarketPlace.Models;
using PagedList;

namespace NoteMarketPlace.Models
{
    public class Download
    {
        public Feedback noteReview { get; set; }
        public SpamReport spamReport { get; set; }
        public IPagedList<BuyerRequest> pagedList { get; set; }
    }
}