using System;

namespace SocketDemo.Models
{
    public class SearchResult
    {
        public int ValuationId { get; set; }
        public DateTime UpdateDate { get; set; }
        public string DataSource { get; set; }
        public string WordBag { get; set; }

        public override string ToString()
        {
            return string.Format($"{ValuationId} {UpdateDate} {DataSource} {WordBag}");
        }
    }
}
