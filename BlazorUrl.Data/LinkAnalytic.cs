using System.ComponentModel.DataAnnotations;

namespace BlazorUrl.Data
{
    public class LinkAnalytic
    {
        [Key]
        public long Id { get; set; }

        public  long LinkId { get; set; }

        public DateTime ClickedAt { get; set; }

        public virtual Link Link { get; set; }

    }
}
